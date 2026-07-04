using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebApp.Repository.IRepository
{
    public interface IExamPeriodAssignmentRepository
    {
        Task<ResponseData<List<ExamPeriodAssignmentModel>>> GetMyAssignmentsAsync(CancellationToken cancellationToken = default);
    }
}
