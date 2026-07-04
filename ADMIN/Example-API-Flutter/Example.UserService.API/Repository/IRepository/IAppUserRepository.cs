using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IAppUserRepository
    {
        Task<IEnumerable<AppUserModel>> GetListPaging(AppUserSearchModel search);
        Task<long> GetTotalRecord(AppUserSearchModel search);
        Task<AppUser> GetByIdAsync(long id);
        Task<long> CreateAsync(AppUser appUser);
        Task UpdateAsync(AppUser appUser);
        Task DeleteAsync(AppUser appUser);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<bool> CheckNameExists(string name, long id);
        Task<bool> CheckUserNameExists(string name, long id);
        Task<bool> CheckEmailExists(string email, long id);
        Task<bool> CheckPhoneExists(string? phone, long id);
        Task<AppUser> GetByUsernameAsync(string username);
        Task<List<string>> GetUserPermissions(long userId);
        Task<bool> IsSuperAdminUser(long userId);
    }
}
