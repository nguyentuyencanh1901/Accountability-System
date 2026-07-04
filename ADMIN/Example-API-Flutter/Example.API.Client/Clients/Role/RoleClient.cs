using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.Role
{
    public class RoleClient : ApiClientBase, IRoleClient
    {
        private const string Controller = "Role";

        public RoleClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<RoleModel>>> GetListAsync(RoleSearchModel search, CancellationToken cancellationToken = default)
            => SendAsync<List<RoleModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-role", BuildSearchQuery(search)), cancellationToken: cancellationToken);

        public Task<ResponseData<RoleModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<RoleModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(RoleSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(RoleSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);
    }
}
