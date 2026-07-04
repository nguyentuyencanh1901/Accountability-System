using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.ExamSession
{
    public class ExamSessionClient : ApiClientBase, IExamSessionClient
    {
        private const string Controller = "ExamSession";

        public ExamSessionClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<ExamSessionModel>>> GetListAsync(ExamSessionSearchModel search, CancellationToken cancellationToken = default)
            => SendAsync<List<ExamSessionModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-examSession", BuildExamSessionQuery(search)), cancellationToken: cancellationToken);

        public Task<ResponseData<List<ExamSessionModel>>> GetHistoryAsync(ExamSessionSearchModel search, CancellationToken cancellationToken = default)
            => SendAsync<List<ExamSessionModel>>(HttpMethod.Get, BuildUri(Controller, "get-history", BuildExamSessionQuery(search)), cancellationToken: cancellationToken);

        public Task<ResponseData<ExamSessionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<ExamSessionModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(ExamSessionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(ExamSessionSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<ExamSessionModel>> StartExamAsync(StartExamModel model, CancellationToken cancellationToken = default)
            => SendAsync<ExamSessionModel>(HttpMethod.Post, BuildUri(Controller, "start-exam"), model, cancellationToken);

        public Task<ResponseData<object>> SaveExamProgressAsync(SubmitExamModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "save-exam-progress"), model, cancellationToken);

        public Task<ResponseData<ExamSessionModel>> SubmitExamAsync(SubmitExamModel model, CancellationToken cancellationToken = default)
            => SendAsync<ExamSessionModel>(HttpMethod.Post, BuildUri(Controller, "submit-exam"), model, cancellationToken);

        public Task<ResponseData<object>> CancelExamDueToViolationAsync(SubmitExamModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "cancel-exam-violation"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        private static Dictionary<string, string?> BuildExamSessionQuery(ExamSessionSearchModel search)
        {
            var query = BuildSearchQuery(search);
            if (search.ExamSetId > 0) query["examSetId"] = search.ExamSetId.ToString();
            if (search.UserId > 0) query["userId"] = search.UserId.ToString();
            if (search.ExamType > 0) query["examType"] = search.ExamType.ToString();
            if (search.Status > 0) query["status"] = search.Status.ToString();
            return query;
        }
    }
}
