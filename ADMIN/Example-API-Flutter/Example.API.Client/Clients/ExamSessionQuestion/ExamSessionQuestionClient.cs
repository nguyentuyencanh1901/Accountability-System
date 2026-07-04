using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.ExamSessionQuestion
{
    public class ExamSessionQuestionClient : ApiClientBase, IExamSessionQuestionClient
    {
        private const string Controller = "ExamSessionQuestion";

        public ExamSessionQuestionClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<ExamSessionQuestionModel>>> GetListAsync(ExamSessionQuestionSearchModel search, CancellationToken cancellationToken = default)
        {
            var query = BuildSearchQuery(search);
            if (search.ExamSessionId > 0) query["examSessionId"] = search.ExamSessionId.ToString();
            if (search.QuestionId > 0) query["questionId"] = search.QuestionId.ToString();
            return SendAsync<List<ExamSessionQuestionModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-examSessionQuestion", query), cancellationToken: cancellationToken);
        }

        public Task<ResponseData<ExamSessionQuestionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<ExamSessionQuestionModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(ExamSessionQuestionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(ExamSessionQuestionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);
    }
}
