using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IExamSessionAnswerOptionRepository
    {
        Task<ExamSessionAnswerOption> GetByIdAsync(long id);
        Task<long> CreateAsync(ExamSessionAnswerOption entity);
        Task UpdateAsync(ExamSessionAnswerOption entity);
        Task DeleteAsync(ExamSessionAnswerOption entity);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<IList<ExamSessionAnswerOption>> GetByExamSessionAnswerIds(List<long> examSessionAnswerIds);
        Task<int> CountByAnswerOptionId(long answerOptionId);
        Task<IList<long>> CreateListAsync(IEnumerable<ExamSessionAnswerOption> entities);
        Task DeleteListAsync(IEnumerable<ExamSessionAnswerOption> entities);
        Task<IEnumerable<ExamSessionAnswerOptionModel>> GetListPaging(ExamSessionAnswerOptionSearchModel search);
        Task<long> GetTotalRecord(ExamSessionAnswerOptionSearchModel search);
    }
}
