using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.ExamPeriodAssignment;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IExamPeriodAssignmentService
    {
        Task<(List<ExamPeriodAssignmentModel> Items, long TotalItems, string? ExamPeriodName, string? ErrorMessage)> GetListPagingAsync(long? examPeriodId, int pageIndex);
        Task<ExamPeriodAssignmentFormViewModel> BuildCreateFormAsync(long? examPeriodId);
        Task<(ExamPeriodAssignmentFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        IEnumerable<(string Key, string Message)> ValidateCreate(ExamPeriodAssignmentFormViewModel model);
        IEnumerable<(string Key, string Message)> ValidateEdit(ExamPeriodAssignmentFormViewModel model);
        Task<ResponseData<object>> InsertBulkAsync(ExamPeriodAssignmentFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(ExamPeriodAssignmentFormViewModel model);
        Task<ResponseData<object>> UpdateStatusAsync(long id, int status);
        Task<ResponseData<object>> DeleteAsync(long id);
        Task PopulateFormOptionsAsync(ExamPeriodAssignmentFormViewModel model);
        string GetExamTypeName(int examType);
        string GetStatusName(int status);
    }
}
