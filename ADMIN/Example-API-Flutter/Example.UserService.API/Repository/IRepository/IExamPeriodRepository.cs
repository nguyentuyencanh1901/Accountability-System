using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IExamPeriodRepository
    {
        Task<IEnumerable<ExamPeriodModel>> GetListPaging(ExamPeriodSearchModel search);
        Task<long> GetTotalRecord(ExamPeriodSearchModel search);
        Task<ExamPeriod> GetByIdAsync(long id);
        Task<long> CreateAsync(ExamPeriod examPeriod);
        Task UpdateAsync(ExamPeriod examPeriod);
        Task DeleteAsync(ExamPeriod examPeriod);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<bool> CheckNameExists(string name, long id);
        Task<List<ExamPeriod>> GetPublishedExpiredAsync(DateTimeOffset now);
    }
}
