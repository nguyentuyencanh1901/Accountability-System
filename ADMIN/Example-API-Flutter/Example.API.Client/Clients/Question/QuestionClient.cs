using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.Question
{
    public class QuestionClient : ApiClientBase, IQuestionClient
    {
        private const string Controller = "Question";

        public QuestionClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<QuestionModel>>> GetListAsync(QuestionSearchModel search, CancellationToken cancellationToken = default)
        {
            var query = BuildSearchQuery(search);
            if (search.QuestionType > 0) query["questionType"] = search.QuestionType.ToString();
            if (search.FieldId > 0) query["fieldId"] = search.FieldId.ToString();
            if (search.DifficultyLevel > 0) query["difficultyLevel"] = search.DifficultyLevel.ToString();
            return SendAsync<List<QuestionModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-question", query), cancellationToken: cancellationToken);
        }

        public Task<ResponseData<QuestionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<QuestionModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(QuestionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(QuestionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateStatusAsync(UpdateStatusModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "update-status"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);
    }
}
