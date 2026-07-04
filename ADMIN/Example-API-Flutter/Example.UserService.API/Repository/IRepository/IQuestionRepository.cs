using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<QuestionModel>> GetListPaging(QuestionSearchModel search);
        Task<long> GetTotalRecord(QuestionSearchModel search);
        Task<Question> GetByIdAsync(long id);
        Task<long> CreateAsync(Question question);
        Task UpdateAsync(Question question);
        Task DeleteAsync(Question question);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<List<Question>> GetActiveQuestionsAsync();
        Task<List<Question>> GetActiveQuestionsByFieldIdsAsync(IEnumerable<long> fieldIds);
    }
}
