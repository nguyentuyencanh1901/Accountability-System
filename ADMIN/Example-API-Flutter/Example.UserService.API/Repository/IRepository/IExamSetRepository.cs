using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IExamSetRepository
    {
        Task<IEnumerable<ExamSetModel>> GetListPaging(ExamSetSearchModel search);
        Task<long> GetTotalRecord(ExamSetSearchModel search);
        Task<ExamSet> GetByIdAsync(long id);
        Task<long> CreateAsync(ExamSet examSet);
        Task UpdateAsync(ExamSet examSet);
        Task DeleteAsync(ExamSet examSet);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<bool> CheckNameExists(string name, long id);
    }
}
