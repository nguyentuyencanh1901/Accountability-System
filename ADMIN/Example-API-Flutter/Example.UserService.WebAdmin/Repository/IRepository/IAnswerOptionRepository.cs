using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebAdmin.Repository.IRepository
{
    public interface IAnswerOptionRepository
    {
        Task<ResponseData<List<AnswerOptionModel>>> GetListAsync(AnswerOptionSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<AnswerOptionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(AnswerOptionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(AnswerOptionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
