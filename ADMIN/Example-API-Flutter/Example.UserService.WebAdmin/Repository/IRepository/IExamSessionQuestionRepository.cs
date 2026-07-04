using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebAdmin.Repository.IRepository
{
    public interface IExamSessionQuestionRepository
    {
        Task<ResponseData<List<ExamSessionQuestionModel>>> GetListAsync(ExamSessionQuestionSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamSessionQuestionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(ExamSessionQuestionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(ExamSessionQuestionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
