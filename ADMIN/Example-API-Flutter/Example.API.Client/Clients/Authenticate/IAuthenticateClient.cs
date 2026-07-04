using Example.API.Client.Core;
using Example.API.Client.Models;
using Example.Common.Models;

namespace Example.API.Client.Clients.Authenticate
{
    /// <summary>Client cho AuthenticateController</summary>
    public interface IAuthenticateClient
    {
        Task<ResponseData<LoginResponse>> LoginAsync(LoginModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<LoginResponse>> LoginAppAsync(LoginModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateProfileAsync(UpdateProfileModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> ChangePasswordAsync(ChangePasswordModel model, CancellationToken cancellationToken = default);
        void Logout();
    }
}
