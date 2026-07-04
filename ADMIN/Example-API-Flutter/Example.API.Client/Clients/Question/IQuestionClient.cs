using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.API.Client.Clients.Question
{
    /// <summary>Client cho QuestionController</summary>
    public interface IQuestionClient
    {
        Task<ResponseData<List<QuestionModel>>> GetListAsync(QuestionSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<QuestionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(QuestionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(QuestionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateStatusAsync(UpdateStatusModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
