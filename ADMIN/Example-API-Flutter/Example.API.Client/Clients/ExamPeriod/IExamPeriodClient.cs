using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.API.Client.Clients.ExamPeriod
{
    /// <summary>Client cho ExamPeriodController</summary>
    public interface IExamPeriodClient
    {
        Task<ResponseData<List<ExamPeriodModel>>> GetListAsync(ExamPeriodSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamPeriodModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamPeriodMonitoringModel>> GetMonitoringAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(ExamPeriodSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(ExamPeriodSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
