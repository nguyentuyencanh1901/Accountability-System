using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.ExamPeriodAssignment
{
    public class ExamPeriodAssignmentClient : ApiClientBase, IExamPeriodAssignmentClient
    {
        private const string Controller = "ExamPeriodAssignment";

        public ExamPeriodAssignmentClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<ExamPeriodAssignmentModel>>> GetListAsync(ExamPeriodAssignmentSearchModel search, CancellationToken cancellationToken = default)
            => SendAsync<List<ExamPeriodAssignmentModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-examPeriodAssignment", BuildExamPeriodAssignmentQuery(search)), cancellationToken: cancellationToken);

        public Task<ResponseData<List<ExamPeriodAssignmentModel>>> GetMyAssignmentsAsync(CancellationToken cancellationToken = default)
            => SendAsync<List<ExamPeriodAssignmentModel>>(HttpMethod.Get, BuildUri(Controller, "get-my-assignments"), cancellationToken: cancellationToken);

        public Task<ResponseData<ExamPeriodAssignmentModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<ExamPeriodAssignmentModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(ExamPeriodAssignmentSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> AddBulkAsync(ExamPeriodAssignmentBulkSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add-bulk"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(ExamPeriodAssignmentSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        private static Dictionary<string, string?> BuildExamPeriodAssignmentQuery(ExamPeriodAssignmentSearchModel search)
        {
            var query = BuildSearchQuery(search);
            if (search.ExamPeriodId > 0) query["examPeriodId"] = search.ExamPeriodId.ToString();
            if (search.UserId > 0) query["userId"] = search.UserId.ToString();
            if (search.ExamSetId > 0) query["examSetId"] = search.ExamSetId.ToString();
            if (search.ExamType > 0) query["examType"] = search.ExamType.ToString();
            return query;
        }
    }
}
