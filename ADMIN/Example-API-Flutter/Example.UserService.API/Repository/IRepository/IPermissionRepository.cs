using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<PermissionModel>> GetListPaging(PermissionSearchModel search);
        Task<long> GetTotalRecord(PermissionSearchModel search);
        Task<Permission> GetByIdAsync(long id);
        Task<long> CreateAsync(Permission permission);
        Task UpdateAsync(Permission permission);
        Task DeleteAsync(Permission permission);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<bool> CheckNameExists(string name, long id);
    }
}
