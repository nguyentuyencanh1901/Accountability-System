using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebAdmin.Repository.IRepository
{
    public interface IExamPeriodAssignmentRepository
    {
        Task<ResponseData<List<ExamPeriodAssignmentModel>>> GetListAsync(ExamPeriodAssignmentSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamPeriodAssignmentModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddBulkAsync(ExamPeriodAssignmentBulkSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(ExamPeriodAssignmentSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
