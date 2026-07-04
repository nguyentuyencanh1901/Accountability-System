using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRoleModel>> GetListPaging(UserRoleSearchModel search);
        Task<long> GetTotalRecord(UserRoleSearchModel search);
        Task<UserRole> GetByIdAsync(long id);
        Task<long> CreateAsync(UserRole userRole);
        Task UpdateAsync(UserRole userRole);
        Task DeleteAsync(UserRole userRole);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();

        Task<IList<UserRole>> GetById(long Id);
        Task<IList<UserRole>> GetByListId(List<long> Ids);
        Task<IList<long>> CreateListAsync(IEnumerable<UserRole> entities);

        Task DeleteListAsync(IEnumerable<UserRole> entities);
    }
}
