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
    /// Repository quản lý vai trò (Role) trong hệ thống phân quyền RBAC.
    /// Kết hợp EF Core cho kiểm tra trùng tên và Dapper cho danh sách kèm số lượng quyền.
    /// </summary>
    public class RoleRepository : RepositoryBaseAsync<Role, long, DataContext>, IRoleRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public RoleRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Kiểm tra tên vai trò trùng — tên role phải duy nhất để tránh nhầm lẫn khi phân quyền.
        /// </summary>
        public Task<bool> CheckNameExists(string name, long id)
        {
            return FindByCondition(x => x.Name == name && x.Id != id).AnyAsync();
        }

        /// <summary>
        /// Danh sách vai trò có phân trang kèm số quyền — LEFT JOIN RolePermissions và GROUP BY để đếm PermissionCount.
        /// </summary>
        public async Task<IEnumerable<RoleModel>> GetListPaging(RoleSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT d.Id, d.Name, COUNT(rp.Id) AS PermissionCount FROM Roles d LEFT JOIN RolePermissions rp ON rp.RoleId = d.Id ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo trạng thái vai trò — 0 nghĩa là lấy tất cả
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            query.Append(" GROUP BY d.Id, d.Name ");
            query.Append(" ORDER BY d.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            IEnumerable<RoleModel> list = await DapperQueryAsync<RoleModel>(query.ToString(), dynamicParameters);
            return list;
        }

        /// <summary>
        /// Đếm tổng vai trò theo bộ lọc — dùng cho phân trang, đồng bộ điều kiện với GetListPaging.
        /// </summary>
        public async Task<long> GetTotalRecord(RoleSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT COUNT(1) FROM Roles d");
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
