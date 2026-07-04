using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebAdmin.Repository.IRepository
{
    public interface IExamSessionAnswerRepository
    {
        Task<ResponseData<List<ExamSessionAnswerModel>>> GetListAsync(ExamSessionAnswerSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamSessionAnswerModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(ExamSessionAnswerSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(ExamSessionAnswerSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
