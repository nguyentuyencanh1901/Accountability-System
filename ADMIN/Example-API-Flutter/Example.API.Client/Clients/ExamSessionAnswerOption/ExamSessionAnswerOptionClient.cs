using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.ExamSessionAnswerOption
{
    public class ExamSessionAnswerOptionClient : ApiClientBase, IExamSessionAnswerOptionClient
    {
        private const string Controller = "ExamSessionAnswerOption";

        public ExamSessionAnswerOptionClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<ExamSessionAnswerOptionModel>>> GetListAsync(ExamSessionAnswerOptionSearchModel search, CancellationToken cancellationToken = default)
        {
            var query = BuildSearchQuery(search);
            if (search.ExamSessionAnswerId > 0) query["examSessionAnswerId"] = search.ExamSessionAnswerId.ToString();
            if (search.AnswerOptionId > 0) query["answerOptionId"] = search.AnswerOptionId.ToString();
            return SendAsync<List<ExamSessionAnswerOptionModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-examSessionAnswerOption", query), cancellationToken: cancellationToken);
        }

        public Task<ResponseData<ExamSessionAnswerOptionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<ExamSessionAnswerOptionModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(ExamSessionAnswerOptionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(ExamSessionAnswerOptionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);
    }
}
