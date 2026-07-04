using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.AppUser;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IAppUserService
    {
        Task<(List<AppUserModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex, int userType);
        Task<AppUserFormViewModel> BuildCreateFormAsync();
        Task<(AppUserFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        IEnumerable<(string Key, string Message)> ValidateCreate(AppUserFormViewModel model);
        Task<ResponseData<object>> InsertAsync(AppUserFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(AppUserFormViewModel model);
        Task<(ResponseData<object>? Result, string? BlockedMessage)> UpdateStatusAsync(long id, int status);
        Task<(ResponseData<object>? Result, string? BlockedMessage)> ResetPasswordAsync(long id, string newPassword);
        Task<(ResetPasswordViewModel? Model, string? ErrorMessage)> GetResetPasswordFormAsync(long id);
        Task<(ResponseData<object>? Result, string? BlockedMessage)> DeleteAsync(long id);
        Task PopulateFormOptionsAsync(AppUserFormViewModel model);
    }
}
