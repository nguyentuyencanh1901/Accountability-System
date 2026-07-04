using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.ExamPeriod
{
    public class ExamPeriodClient : ApiClientBase, IExamPeriodClient
    {
        private const string Controller = "ExamPeriod";

        public ExamPeriodClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<ExamPeriodModel>>> GetListAsync(ExamPeriodSearchModel search, CancellationToken cancellationToken = default)
            => SendAsync<List<ExamPeriodModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-examPeriod", BuildExamPeriodQuery(search)), cancellationToken: cancellationToken);

        public Task<ResponseData<ExamPeriodModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<ExamPeriodModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<ExamPeriodMonitoringModel>> GetMonitoringAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<ExamPeriodMonitoringModel>(HttpMethod.Get, BuildUri(Controller, "get-monitoring", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(ExamPeriodSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(ExamPeriodSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        private static Dictionary<string, string?> BuildExamPeriodQuery(ExamPeriodSearchModel search)
        {
            var query = BuildSearchQuery(search);
            return query;
        }
    }
}
