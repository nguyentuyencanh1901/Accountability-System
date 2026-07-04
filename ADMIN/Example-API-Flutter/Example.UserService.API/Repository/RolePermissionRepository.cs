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
    /// Repository quản lý liên kết vai trò–quyền (RolePermission) trong mô hình RBAC.
    /// Kết hợp EF Core cho tra cứu theo role và Dapper cho danh sách phân trang.
    /// </summary>
    public class RolePermissionRepository : RepositoryBaseAsync<RolePermission, long, DataContext>, IRolePermissionRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public RolePermissionRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Lấy tất cả quyền gán cho một vai trò — dùng khi chỉnh sửa role hoặc kiểm tra authorization.
        /// </summary>
        public async Task<IList<RolePermission>> GetById(long Id)
        {
            return await FindByCondition(x => x.RoleId == Id).ToListAsync();
        }

        /// <summary>
        /// Lấy quyền của nhiều vai trò cùng lúc — tối ưu khi load quyền cho danh sách role.
        /// </summary>
        public async Task<IList<RolePermission>> GetByListId(List<long> Ids)
        {
            return await FindByCondition(x => Ids.Contains(x.RoleId)).ToListAsync();
        }

        /// <summary>
        /// Danh sách liên kết role–permission có phân trang — Dapper cho truy vấn đơn giản hiệu năng cao.
        /// </summary>
        public async Task<IEnumerable<RolePermissionModel>> GetListPaging(RolePermissionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT d.Id, d.RoleId, d.PermissionId FROM RolePermissions d ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo trạng thái liên kết — 0 nghĩa là lấy tất cả
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            query.Append(" ORDER BY d.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            IEnumerable<RolePermissionModel> list = await DapperQueryAsync<RolePermissionModel>(query.ToString(), dynamicParameters);
            return list;
        }

        /// <summary>
        /// Đếm tổng bản ghi liên kết theo bộ lọc — dùng cho phân trang.
        /// </summary>
        public async Task<long> GetTotalRecord(RolePermissionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT COUNT(1) FROM RolePermissions d");
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
