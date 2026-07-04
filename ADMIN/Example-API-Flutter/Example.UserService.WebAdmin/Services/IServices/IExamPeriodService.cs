using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.ExamPeriod;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IExamPeriodService
    {
        Task<(List<ExamPeriodModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex);
        Task<ExamPeriodFormViewModel> BuildCreateFormAsync();
        Task<(ExamPeriodFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        IEnumerable<(string Key, string Message)> ValidateSelections(ExamPeriodFormViewModel model);
        Task<ResponseData<object>> InsertAsync(ExamPeriodFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(ExamPeriodFormViewModel model);
        Task<ResponseData<object>> UpdateStatusAsync(long id, int status);
        Task<ResponseData<object>> DeleteAsync(long id);
        Task PopulateFormOptionsAsync(ExamPeriodFormViewModel model);
        Task<List<Example.UserService.WebAdmin.Models.Shared.ExamSetPickOptionViewModel>> GetExamSetOptionsByTypeAsync(int examSetType, List<long>? selectedIds = null);
        string GetStatusName(int status);
        Task<(ExamPeriodDetailViewModel? Model, string? ErrorMessage)> GetMonitoringAsync(long id, int? status, int? examType);
        string FormatDuration(DateTimeOffset? startedAt, DateTimeOffset? finishedAt);
    }
}
