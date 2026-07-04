using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebAdmin.Repository.IRepository
{
    public interface IExamPeriodRepository
    {
        Task<ResponseData<List<ExamPeriodModel>>> GetListAsync(ExamPeriodSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamPeriodModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamPeriodMonitoringModel>> GetMonitoringAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(ExamPeriodSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(ExamPeriodSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
