using Example.API.Client.Models;
using Example.Common.Models;

namespace Example.UserService.WebApp.Repository.IRepository
{
    public interface IAuthenticateRepository
    {
        Task<ResponseData<LoginResponse>> LoginAppAsync(LoginModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateProfileAsync(UpdateProfileModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> ChangePasswordAsync(ChangePasswordModel model, CancellationToken cancellationToken = default);
        void Logout();
    }
}
