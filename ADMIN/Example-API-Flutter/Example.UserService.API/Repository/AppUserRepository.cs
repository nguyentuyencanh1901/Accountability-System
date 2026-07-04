using Example.Common.Repository.Interfaces;
using Example.Common.Repository;
using Example.UserService.API.DBContexts;
using Example.UserService.API.Entities;
using Example.UserService.API.Repository.IRepository;
using Example.UserService.API.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Example.Common.Utilities;
using System.Xml.Linq;
using Elastic.Apm.Api;

namespace Example.UserService.API.Repository
{
    /// <summary>
    /// Repository truy cập dữ liệu người dùng (Users), hỗ trợ kiểm tra trùng lặp, phân trang và tra cứu quyền.
    /// Kết hợp EF Core cho tra cứu đăng nhập và Dapper cho danh sách admin cùng truy vấn quyền RBAC.
    /// </summary>
    public class AppUserRepository : RepositoryBaseAsync<AppUser, long, DataContext>, IAppUserRepository
    {
        /// <summary>Inject DbContext và UnitOfWork từ lớp cơ sở để tái sử dụng các thao tác CRUD chuẩn.</summary>
        public AppUserRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Kiểm tra username đã tồn tại ở bản ghi khác — dùng khi tạo/sửa để tránh trùng đăng nhập.
        /// </summary>
        public Task<bool> CheckUserNameExists(string name, long id)
        {
            // Loại trừ bản ghi hiện tại (id) để cho phép giữ nguyên username khi cập nhật
            return FindByCondition(x => x.Username == name && x.Id != id).AnyAsync();
        }

        /// <summary>
        /// Kiểm tra họ tên đã tồn tại — đảm bảo tính duy nhất của FullName trong hệ thống.
        /// </summary>
        public Task<bool> CheckNameExists(string name, long id)
        {
            return FindByCondition(x => x.FullName == name && x.Id != id).AnyAsync();
        }

        /// <summary>
        /// Kiểm tra email đã được đăng ký bởi người dùng khác — tránh trùng email khi reset mật khẩu/liên hệ.
        /// </summary>
        public Task<bool> CheckEmailExists(string email, long id)
        {
            return FindByCondition(x => x.Email == email && x.Id != id).AnyAsync();

        }

        /// <summary>
        /// Kiểm tra số điện thoại trùng lặp — phone là tùy chọn nên cần bỏ qua khi rỗng.
        /// </summary>
        public Task<bool> CheckPhoneExists(string? phone, long id)
        {
            // Không kiểm tra khi phone null/rỗng vì trường này không bắt buộc, tránh false positive
            if (string.IsNullOrWhiteSpace(phone))
                return Task.FromResult(false);

            return FindByCondition(x => x.Phone == phone && x.Id != id).AnyAsync();
        }

        /// <summary>
        /// Lấy danh sách người dùng có phân trang qua Dapper — tối ưu hiệu năng so với EF cho truy vấn phức tạp.
        /// </summary>
        public async Task<IEnumerable<AppUserModel>> GetListPaging(AppUserSearchModel search)
        {
            // Tham số động để tránh SQL injection khi ghép điều kiện lọc
            DynamicParameters dynamicParameters = new DynamicParameters();
            // Truy vấn SELECT cơ bản lấy các cột cần thiết cho danh sách quản trị
            StringBuilder query = new StringBuilder(@" SELECT d.Id, d.Username, d.PasswordHash, d.Email, d.Phone, d.FullName, d.Status, d.UserType FROM Users d ");
            // WHERE 1=1 giúp nối thêm điều kiện AND mà không cần xử lý logic phức tạp
            query.Append(" WHERE 1 = 1");
            // Chỉ lọc theo Status khi giá trị > 0 — 0 nghĩa là "tất cả trạng thái"
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            // Lọc theo loại người dùng (admin/thí sinh...) khi có chọn cụ thể
            if (search.UserType > 0)
            {
                query.Append(" AND d.UserType = @UserType");
                dynamicParameters.Add("UserType", search.UserType);
            }
            // Sắp xếp mới nhất trước để admin thấy tài khoản vừa tạo ở đầu danh sách
            query.Append(" ORDER BY d.Id DESC ");
            // Thêm OFFSET/FETCH phân trang theo PageIndex và PageSize từ model tìm kiếm
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            IEnumerable<AppUserModel> list = await DapperQueryAsync<AppUserModel>(query.ToString(), dynamicParameters);
            return list;
        }

        /// <summary>
        /// Đếm tổng bản ghi khớp bộ lọc — dùng cho tính tổng số trang trên UI phân trang.
        /// </summary>
        public async Task<long> GetTotalRecord(AppUserSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            // COUNT thay vì SELECT đầy đủ để giảm tải DB khi chỉ cần số lượng
            StringBuilder query = new StringBuilder(@" SELECT COUNT(1) FROM Users d ");
            query.Append(" WHERE 1 = 1");
            // Áp dụng cùng bộ lọc với GetListPaging để số trang khớp với dữ liệu thực tế
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            if (search.UserType > 0)
            {
                query.Append(" AND d.UserType = @UserType");
                dynamicParameters.Add("UserType", search.UserType);
            }
            var totalRecord = await DapperGetAsync<int>(query.ToString(), dynamicParameters);
            return totalRecord;
        }

        /// <summary>
        /// Tra cứu người dùng theo username — phục vụ đăng nhập và xác thực.
        /// </summary>
        public Task<AppUser?> GetByUsernameAsync(string username)
        {
            return FindByCondition(x => x.Username == username)
                    .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy danh sách mã quyền (permission code) của user qua chuỗi Role → RolePermission → Permission.
        /// </summary>
        public async Task<List<string>> GetUserPermissions(long userId)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();

            // JOIN 4 bảng để gom quyền từ mọi vai trò mà user được gán; DISTINCT tránh trùng khi nhiều role cùng quyền
            StringBuilder query = new StringBuilder(@" SELECT DISTINCT p.Code FROM UserRoles ur
                                                    INNER JOIN Roles r ON ur.RoleId = r.Id
                                                    INNER JOIN RolePermissions rp ON r.Id = rp.RoleId
                                                    INNER JOIN Permissions p ON rp.PermissionId = p.Id
                                                                ");

            query.Append(" WHERE ur.UserId = @UserId");
            dynamicParameters.Add("UserId", userId);

            IEnumerable<string> list = await DapperQueryAsync<string>(query.ToString(), dynamicParameters);

            return list.ToList();
        }

        /// <summary>
        /// Xác định user có phải Super Admin không — bypass một số kiểm tra quyền ở tầng service.
        /// </summary>
        public async Task<bool> IsSuperAdminUser(long userId)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            // Kiểm tra cả mã quyền và tên role vì hệ thống có thể cấu hình Super Admin theo nhiều cách khác nhau
            const string query = @"
                SELECT COUNT(1)
                FROM UserRoles ur
                INNER JOIN Roles r ON ur.RoleId = r.Id
                LEFT JOIN RolePermissions rp ON r.Id = rp.RoleId
                LEFT JOIN Permissions p ON rp.PermissionId = p.Id
                WHERE ur.UserId = @UserId
                  AND (
                        UPPER(p.Code) IN ('SUPER_ADMIN', 'SUPER_ADMIN')
                     OR UPPER(r.Name) IN ('SUPERADMIN', 'SUPER_ADMIN', 'SUPER ADMIN', 'SUPER_ADMIN')
                  )";

            dynamicParameters.Add("UserId", userId);
            var count = await DapperGetAsync<int>(query, dynamicParameters);
            // COUNT > 0 nghĩa là user sở hữu ít nhất một role/quyền Super Admin
            return count > 0;
        }
    }
}
