using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.API.Client.Clients.ExamSessionAnswerOption
{
    /// <summary>Client cho ExamSessionAnswerOptionController</summary>
    public interface IExamSessionAnswerOptionClient
    {
        Task<ResponseData<List<ExamSessionAnswerOptionModel>>> GetListAsync(ExamSessionAnswerOptionSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamSessionAnswerOptionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(ExamSessionAnswerOptionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(ExamSessionAnswerOptionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
