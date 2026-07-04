using Example.UserService.API.Entities;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IExamPeriodExamSetRepository
    {
        Task<List<long>> GetExamSetIdsByPeriodIdAsync(long examPeriodId);
        Task<List<ExamPeriodExamSet>> GetByPeriodIdAsync(long examPeriodId);
        Task<IList<long>> CreateListAsync(IEnumerable<ExamPeriodExamSet> items);
        Task DeleteListAsync(IEnumerable<ExamPeriodExamSet> items);
        Task<int> SaveChangesAsync();
    }
}
