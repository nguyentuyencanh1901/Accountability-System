using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.API.Client.Models;
using Example.Common.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.Authenticate
{
    public class AuthenticateClient : ApiClientBase, IAuthenticateClient
    {
        private const string Controller = "Authenticate";

        public AuthenticateClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<LoginResponse>> LoginAsync(LoginModel model, CancellationToken cancellationToken = default)
            => LoginInternalAsync("login", model, cancellationToken);

        public Task<ResponseData<LoginResponse>> LoginAppAsync(LoginModel model, CancellationToken cancellationToken = default)
            => LoginInternalAsync("login-app", model, cancellationToken);

        private async Task<ResponseData<LoginResponse>> LoginInternalAsync(string action, LoginModel model, CancellationToken cancellationToken)
        {
            var result = await SendAsync<LoginResponse>(HttpMethod.Post, BuildUri(Controller, action), model, cancellationToken);
            if (result.Success && !string.IsNullOrWhiteSpace(result.Data?.Token))
                TokenProvider.Token = result.Data.Token;
            return result;
        }

        public Task<ResponseData<object>> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "register"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateProfileAsync(UpdateProfileModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update-profile"), model, cancellationToken);

        public Task<ResponseData<object>> ChangePasswordAsync(ChangePasswordModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "change-password"), model, cancellationToken);

        public void Logout() => TokenProvider.Clear();
    }
}
