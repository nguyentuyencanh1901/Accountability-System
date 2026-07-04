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
    /// Repository quản lý quyền hạn (Permission) trong hệ thống phân quyền RBAC.
    /// Kết hợp EF Core cho kiểm tra trùng mã quyền và Dapper cho danh sách phân trang.
    /// </summary>
    public class PermissionRepository : RepositoryBaseAsync<Permission, long, DataContext>, IPermissionRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public PermissionRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Kiểm tra mã quyền (Code) trùng — Code là định danh duy nhất dùng trong authorization.
        /// </summary>
        public Task<bool> CheckNameExists(string name, long id)
        {
            return FindByCondition(x => x.Code == name && x.Id != id).AnyAsync();
        }

        /// <summary>
        /// Danh sách quyền có phân trang — Dapper cho truy vấn đơn giản hiệu năng cao.
        /// </summary>
        public async Task<IEnumerable<PermissionModel>> GetListPaging(PermissionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT d.Id, d.Code, d.Description FROM Permissions d ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo trạng thái quyền — 0 nghĩa là lấy tất cả
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            query.Append(" ORDER BY d.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            IEnumerable<PermissionModel> list = await DapperQueryAsync<PermissionModel>(query.ToString(), dynamicParameters);
            return list;
        }

        /// <summary>
        /// Đếm tổng quyền theo bộ lọc — đồng bộ với GetListPaging.
        /// </summary>
        public async Task<long> GetTotalRecord(PermissionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT COUNT(1) FROM Permissions ");
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
