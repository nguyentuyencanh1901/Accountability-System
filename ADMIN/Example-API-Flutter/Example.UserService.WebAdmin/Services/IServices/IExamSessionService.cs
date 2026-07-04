using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.ExamSession;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IExamSessionService
    {
        Task<(List<ExamSessionModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex);
        Task<ExamSessionFormViewModel> BuildCreateFormAsync();
        Task<(ExamSessionFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        Task<(ExamSessionDetailViewModel Model, string? ErrorMessage)> GetDetailsAsync(long id);
        Task<ResponseData<object>> InsertAsync(ExamSessionFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(ExamSessionFormViewModel model);
        Task<ResponseData<object>> UpdateStatusAsync(long id, int status);
        Task<ResponseData<object>> DeleteAsync(long id);
        Task PopulateFormOptionsAsync(ExamSessionFormViewModel model);
        string GetExamTypeName(int examType);
        string GetStatusName(int status);
    }
}
