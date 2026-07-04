using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.AppUser
{
    public class AppUserClient : ApiClientBase, IAppUserClient
    {
        private const string Controller = "AppUser";

        public AppUserClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<AppUserModel>>> GetListAsync(AppUserSearchModel search, CancellationToken cancellationToken = default)
        {
            var query = BuildSearchQuery(search);
            if (search.UserType > 0) query["userType"] = search.UserType.ToString();
            return SendAsync<List<AppUserModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-appUser", query), cancellationToken: cancellationToken);
        }

        public Task<ResponseData<AppUserModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<AppUserModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(AppUserSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(AppUserSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<object>> ResetPasswordAsync(ResetPasswordModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "reset-password"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);
    }
}
