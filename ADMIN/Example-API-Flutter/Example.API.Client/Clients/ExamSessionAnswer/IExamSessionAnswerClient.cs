using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.API.Client.Clients.ExamSessionAnswer
{
    /// <summary>Client cho ExamSessionAnswerController</summary>
    public interface IExamSessionAnswerClient
    {
        Task<ResponseData<List<ExamSessionAnswerModel>>> GetListAsync(ExamSessionAnswerSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamSessionAnswerModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(ExamSessionAnswerSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(ExamSessionAnswerSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
