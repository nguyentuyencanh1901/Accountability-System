using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebAdmin.Repository.IRepository
{
    public interface IExamSessionAnswerOptionRepository
    {
        Task<ResponseData<List<ExamSessionAnswerOptionModel>>> GetListAsync(ExamSessionAnswerOptionSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamSessionAnswerOptionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(ExamSessionAnswerOptionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(ExamSessionAnswerOptionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
