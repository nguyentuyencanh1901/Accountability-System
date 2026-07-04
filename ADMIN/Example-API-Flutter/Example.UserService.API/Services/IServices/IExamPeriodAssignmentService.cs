using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IExamPeriodAssignmentService
    {
        Task<ResponseData<IEnumerable<ExamPeriodAssignmentModel>>> GetListPaging(ExamPeriodAssignmentSearchModel search);
        Task<ResponseData<ExamPeriodAssignmentModel>> GetById(long id);
        Task<ResponseData<IEnumerable<ExamPeriodAssignmentModel>>> GetMyAssignments(long userId);
        Task<ResponseData<object>> Insert(ExamPeriodAssignmentSaveModel model);
        Task<ResponseData<object>> InsertBulk(ExamPeriodAssignmentBulkSaveModel model);
        Task<ResponseData<object>> Update(ExamPeriodAssignmentSaveModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
