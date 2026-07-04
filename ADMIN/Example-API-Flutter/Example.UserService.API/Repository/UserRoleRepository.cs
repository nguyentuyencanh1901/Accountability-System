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

namespace Example.UserService.API.Repository
{
    /// <summary>
    /// Repository quản lý liên kết người dùng–vai trò (UserRole) trong hệ thống phân quyền RBAC.
    /// Kết hợp EF Core cho tra cứu theo user và Dapper cho danh sách phân trang.
    /// </summary>
    public class UserRoleRepository : RepositoryBaseAsync<UserRole, long, DataContext>, IUserRoleRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public UserRoleRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Lấy tất cả vai trò của một người dùng — dùng khi gán/sửa role hoặc kiểm tra quyền.
        /// </summary>
        public async Task<IList<UserRole>> GetById(long Id)
        {
            return await FindByCondition(x => x.UserId == Id).ToListAsync();
        }

        /// <summary>
        /// Lấy vai trò của nhiều user cùng lúc — tránh query lặp khi xử lý danh sách.
        /// </summary>
        public async Task<IList<UserRole>> GetByListId(List<long> Ids)
        {
            return await FindByCondition(x => Ids.Contains(x.UserId)).ToListAsync();
        }

        /// <summary>
        /// Danh sách liên kết user–role có phân trang — Dapper cho truy vấn đơn giản hiệu năng cao.
        /// </summary>
        public async Task<IEnumerable<UserRoleModel>> GetListPaging(UserRoleSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT d.Id, d.RoleId, d.UserId FROM UserRoles d ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo trạng thái liên kết — 0 nghĩa là lấy tất cả
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            query.Append(" ORDER BY d.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            IEnumerable<UserRoleModel> list = await DapperQueryAsync<UserRoleModel>(query.ToString(), dynamicParameters);
            return list;
        }

        /// <summary>
        /// Đếm tổng bản ghi liên kết theo bộ lọc — dùng cho phân trang.
        /// </summary>
        public async Task<long> GetTotalRecord(UserRoleSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT COUNT(1) FROM UserRoles d");
            query.Append(" WHERE 1 = 1");
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            var totalRecord = await DapperGetAsync<int>(query.ToString(), dynamicParameters);
            return totalRecord;
        }
    }
}
