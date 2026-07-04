using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.API.Client.Clients.ExamPeriodAssignment
{
    /// <summary>Client cho ExamPeriodAssignmentController</summary>
    public interface IExamPeriodAssignmentClient
    {
        Task<ResponseData<List<ExamPeriodAssignmentModel>>> GetListAsync(ExamPeriodAssignmentSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<List<ExamPeriodAssignmentModel>>> GetMyAssignmentsAsync(CancellationToken cancellationToken = default);
        Task<ResponseData<ExamPeriodAssignmentModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(ExamPeriodAssignmentSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddBulkAsync(ExamPeriodAssignmentBulkSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(ExamPeriodAssignmentSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
