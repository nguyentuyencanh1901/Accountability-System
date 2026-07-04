using Example.API.Client.Models;
using Example.Common.Models;
using Example.UserService.WebAdmin.Models;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IAuthenticateService
    {
        Task<(bool Success, string? ErrorMessage, LoginResponse? Data)> LoginAsync(LoginViewModel model);
        Task<RegisterViewModel> BuildRegisterViewModelAsync(RegisterViewModel model);
        Task ApplyRegisterDefaultsAsync(RegisterViewModel model);
        IEnumerable<(string Key, string Message)> ValidateRegister(RegisterViewModel model);
        Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordViewModel model);
        void Logout();
    }
}
