using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IExamSessionQuestionRepository
    {
        Task<ExamSessionQuestion> GetByIdAsync(long id);
        Task<long> CreateAsync(ExamSessionQuestion entity);
        Task UpdateAsync(ExamSessionQuestion entity);
        Task DeleteAsync(ExamSessionQuestion entity);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<IList<ExamSessionQuestion>> GetByExamSessionId(long examSessionId);
        Task<IList<long>> CreateListAsync(IEnumerable<ExamSessionQuestion> entities);
        Task DeleteListAsync(IEnumerable<ExamSessionQuestion> entities);
        Task<IEnumerable<ExamSessionQuestionModel>> GetListPaging(ExamSessionQuestionSearchModel search);
        Task<long> GetTotalRecord(ExamSessionQuestionSearchModel search);
    }
}
