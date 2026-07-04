using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.AnswerOption
{
    public class AnswerOptionClient : ApiClientBase, IAnswerOptionClient
    {
        private const string Controller = "AnswerOption";

        public AnswerOptionClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<AnswerOptionModel>>> GetListAsync(AnswerOptionSearchModel search, CancellationToken cancellationToken = default)
        {
            var query = BuildSearchQuery(search);
            if (search.QuestionId > 0) query["questionId"] = search.QuestionId.ToString();
            return SendAsync<List<AnswerOptionModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-answerOption", query), cancellationToken: cancellationToken);
        }

        public Task<ResponseData<AnswerOptionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<AnswerOptionModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(AnswerOptionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(AnswerOptionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);
    }
}
