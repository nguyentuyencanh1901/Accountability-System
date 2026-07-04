using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebAdmin.Repository.IRepository
{
    public interface IExamSessionRepository
    {
        Task<ResponseData<List<ExamSessionModel>>> GetListAsync(ExamSessionSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamSessionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(ExamSessionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(ExamSessionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
