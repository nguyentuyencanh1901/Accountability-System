using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;
using Microsoft.Extensions.Options;

namespace Example.API.Client.Clients.ExamSet
{
    public class ExamSetClient : ApiClientBase, IExamSetClient
    {
        private const string Controller = "ExamSet";

        public ExamSetClient(HttpClient httpClient, IOptions<ApiClientOptions> options, IApiTokenProvider tokenProvider)
            : base(httpClient, options, tokenProvider) { }

        public Task<ResponseData<List<ExamSetModel>>> GetListAsync(ExamSetSearchModel search, CancellationToken cancellationToken = default)
        {
            var query = BuildSearchQuery(search);
            if (search.Type > 0) query["type"] = search.Type.ToString();
            return SendAsync<List<ExamSetModel>>(HttpMethod.Get, BuildUri(Controller, "get-list-examSet", query), cancellationToken: cancellationToken);
        }

        public Task<ResponseData<ExamSetModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<ExamSetModel>(HttpMethod.Get, BuildUri(Controller, "get-by-id", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<object>> AddAsync(ExamSetSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Post, BuildUri(Controller, "add"), model, cancellationToken);

        public Task<ResponseData<object>> UpdateAsync(ExamSetSaveModel model, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Put, BuildUri(Controller, "update"), model, cancellationToken);

        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, BuildUri(Controller, "delete", new Dictionary<string, string?> { ["id"] = id.ToString() }), cancellationToken: cancellationToken);

        public Task<ResponseData<List<ExamSetTakerModel>>> GetExamTakersAsync(ExamSetTakerSearchModel search, CancellationToken cancellationToken = default)
        {
            var query = BuildSearchQuery(search);
            query["examSetId"] = search.ExamSetId.ToString();
            return SendAsync<List<ExamSetTakerModel>>(HttpMethod.Get, BuildUri(Controller, "get-exam-takers", query), cancellationToken: cancellationToken);
        }
    }
}
