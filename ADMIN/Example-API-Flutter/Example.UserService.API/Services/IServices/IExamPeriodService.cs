using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IExamPeriodService
    {
        Task<ResponseData<IEnumerable<ExamPeriodModel>>> GetListPaging(ExamPeriodSearchModel search);
        Task<ResponseData<ExamPeriodModel>> GetById(long id);
        Task<ResponseData<object>> Insert(ExamPeriodSaveModel model);
        Task<ResponseData<object>> Update(ExamPeriodSaveModel model);
        Task<ResponseData<object>> Delete(long id);
        Task CloseExpiredExamPeriodsAsync();
        Task<ResponseData<ExamPeriodMonitoringModel>> GetMonitoring(long id);
    }
}
