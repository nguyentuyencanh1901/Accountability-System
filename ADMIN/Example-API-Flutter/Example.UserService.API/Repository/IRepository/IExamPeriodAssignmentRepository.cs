using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IExamPeriodAssignmentRepository
    {
        Task<IEnumerable<ExamPeriodAssignmentModel>> GetListPaging(ExamPeriodAssignmentSearchModel search);
        Task<long> GetTotalRecord(ExamPeriodAssignmentSearchModel search);
        Task<ExamPeriodAssignment> GetByIdAsync(long id);
        Task<long> CreateAsync(ExamPeriodAssignment assignment);
        Task UpdateAsync(ExamPeriodAssignment assignment);
        Task DeleteAsync(ExamPeriodAssignment assignment);
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<bool> CheckAssignmentExists(long examPeriodId, long userId, int examType, long id);
        Task<List<long>> GetUserIdsByPeriodIdAsync(long examPeriodId);
        Task<List<ExamPeriodAssignment>> GetByPeriodIdAsync(long examPeriodId);
        Task DeleteListAsync(IEnumerable<ExamPeriodAssignment> items);
        Task<long> CountByExamPeriodId(long examPeriodId);
        Task<ExamPeriodAssignment?> GetUserAssignmentAsync(long userId, long assignmentId);
        Task<ExamPeriodAssignment?> GetActiveByUserExamSetTypeAsync(long userId, long examSetId, int examType);
        Task<IEnumerable<ExamPeriodAssignmentModel>> GetMyAssignmentsAsync(long userId);
        Task<IEnumerable<ExamPeriodAssignmentModel>> GetByPeriodIdWithSessionAsync(long examPeriodId);
    }
}
