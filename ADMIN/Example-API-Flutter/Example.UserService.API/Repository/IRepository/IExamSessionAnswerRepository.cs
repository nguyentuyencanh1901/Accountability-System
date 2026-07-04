using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IExamSessionAnswerRepository
    {
        Task<ExamSessionAnswer> GetByIdAsync(long id);
        Task<long> CreateAsync(ExamSessionAnswer entity);
        Task UpdateAsync(ExamSessionAnswer entity);
        Task DeleteAsync(ExamSessionAnswer entity);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<IList<ExamSessionAnswer>> GetByExamSessionId(long examSessionId);
        Task<IList<long>> CreateListAsync(IEnumerable<ExamSessionAnswer> entities);
        Task DeleteListAsync(IEnumerable<ExamSessionAnswer> entities);
        Task<IEnumerable<ExamSessionAnswerModel>> GetListPaging(ExamSessionAnswerSearchModel search);
        Task<long> GetTotalRecord(ExamSessionAnswerSearchModel search);
    }
}
