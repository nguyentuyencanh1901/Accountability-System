using Example.API.Client.Models;
using Example.UserService.WebApp.Models;

namespace Example.UserService.WebApp.Services.IServices
{
    public interface IAuthenticateService
    {
        Task<(bool Success, string? ErrorMessage, LoginResponse? Data)> LoginAsync(LoginViewModel model);
        Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterViewModel model);
        void Logout();
    }
}
