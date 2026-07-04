using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IRoleRepository
    {
        Task<IEnumerable<RoleModel>> GetListPaging(RoleSearchModel search);
        Task<long> GetTotalRecord(RoleSearchModel search);
        Task<Role> GetByIdAsync(long id);
        Task<long> CreateAsync(Role role);
        Task UpdateAsync(Role role);
        Task DeleteAsync(Role role);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<bool> CheckNameExists(string name, long id);

    }
}
