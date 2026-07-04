using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IRolePermissionRepository
    {
        Task<IEnumerable<RolePermissionModel>> GetListPaging(RolePermissionSearchModel search);
        Task<long> GetTotalRecord(RolePermissionSearchModel search);
        Task<RolePermission> GetByIdAsync(long id);
        Task<long> CreateAsync(RolePermission rolePermission);
        Task UpdateAsync(RolePermission rolePermission);
        Task DeleteAsync(RolePermission rolePermission);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();

        Task<IList<RolePermission>> GetById(long Id);
        Task<IList<RolePermission>> GetByListId(List<long> Ids);
        Task<IList<long>> CreateListAsync(IEnumerable<RolePermission> entities);

        Task DeleteListAsync(IEnumerable<RolePermission> entities);
    }
}
