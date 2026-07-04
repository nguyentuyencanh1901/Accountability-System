using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.Permission
{
    public class PermissionClient : ApiClientBase, IPermissionClient
    {
        private const string Controller = "Permission";

        public PermissionClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<PermissionModel>>> GetListAsync(PermissionSearchModel search, CancellationToken cancellationToken = default)
            => SendAsync<List<PermissionModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-permission", BuildSearchQuery(search)), cancellationToken: cancellationToken);

        public Task<ResponseData<PermissionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<PermissionModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(PermissionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(PermissionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);
    }
}
