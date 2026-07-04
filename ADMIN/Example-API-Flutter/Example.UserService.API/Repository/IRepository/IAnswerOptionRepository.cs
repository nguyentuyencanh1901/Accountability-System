using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IAnswerOptionRepository
    {
        Task<AnswerOption> GetByIdAsync(long id);
        Task<long> CreateAsync(AnswerOption answerOption);
        Task UpdateAsync(AnswerOption answerOption);
        Task DeleteAsync(AnswerOption answerOption);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<IList<AnswerOption>> GetByQuestionId(long questionId);
        Task<IList<AnswerOption>> GetByQuestionIds(List<long> questionIds);
        Task<IList<long>> CreateListAsync(IEnumerable<AnswerOption> entities);
        Task DeleteListAsync(IEnumerable<AnswerOption> entities);
        Task<IEnumerable<AnswerOptionModel>> GetListPaging(AnswerOptionSearchModel search);
        Task<long> GetTotalRecord(AnswerOptionSearchModel search);
    }
}
