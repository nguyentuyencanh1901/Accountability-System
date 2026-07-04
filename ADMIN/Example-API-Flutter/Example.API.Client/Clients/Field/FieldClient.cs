using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.Field
{
    public class FieldClient : ApiClientBase, IFieldClient
    {
        private const string Controller = "Field";

        public FieldClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<FieldModel>>> GetListAsync(FieldSearchModel search, CancellationToken cancellationToken = default)
            => SendAsync<List<FieldModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-field", BuildSearchQuery(search)), cancellationToken: cancellationToken);

        public Task<ResponseData<FieldModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<FieldModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(FieldSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(FieldSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateStatusAsync(UpdateStatusModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "update-status"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);
    }
}
