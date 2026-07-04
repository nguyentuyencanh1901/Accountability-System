using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebApp.Repository.IRepository
{
    public interface IExamSessionRepository
    {
        Task<ResponseData<List<ExamSessionModel>>> GetListAsync(ExamSessionSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<List<ExamSessionModel>>> GetHistoryAsync(ExamSessionSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamSessionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamSessionModel>> StartExamAsync(StartExamModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> SaveExamProgressAsync(SubmitExamModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamSessionModel>> SubmitExamAsync(SubmitExamModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> CancelExamDueToViolationAsync(SubmitExamModel model, CancellationToken cancellationToken = default);
    }
}
