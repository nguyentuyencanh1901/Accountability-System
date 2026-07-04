using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IFieldRepository
    {
        Task<IEnumerable<FieldModel>> GetListPaging(FieldSearchModel search);
        Task<long> GetTotalRecord(FieldSearchModel search);
        Task<Field> GetByIdAsync(long id);
        Task<long> CreateAsync(Field field);
        Task UpdateAsync(Field field);
        Task DeleteAsync(Field field);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<bool> CheckNameExists(string name, long id);
        Task<long> CountQuestionsByFieldIdAsync(long fieldId);
    }
}
