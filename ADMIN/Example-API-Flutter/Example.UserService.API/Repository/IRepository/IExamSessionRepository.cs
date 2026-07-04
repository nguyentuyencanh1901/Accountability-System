using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IExamSessionRepository
    {
        Task<IEnumerable<ExamSessionModel>> GetListPaging(ExamSessionSearchModel search);
        Task<long> GetTotalRecord(ExamSessionSearchModel search);
        Task<ExamSession> GetByIdAsync(long id);
        Task<long> CreateAsync(ExamSession examSession);
        Task UpdateAsync(ExamSession examSession);
        Task DeleteAsync(ExamSession examSession);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<long> CountByUserId(long userId);
        Task<ExamSession?> GetInProgressAsync(long userId, long examSetId, int examType);
        Task<ExamSession?> GetInProgressByAssignmentAsync(long assignmentId);
        Task<List<ExamSession>> GetInProgressByUserAsync(long userId);
        Task<IEnumerable<ExamSetTakerModel>> GetTakersByExamSet(ExamSetTakerSearchModel search);
        Task<long> GetTakersTotalByExamSet(ExamSetTakerSearchModel search);
    }
}
