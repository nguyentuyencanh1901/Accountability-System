using Example.API.Client.Configuration;
using Example.Common.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Example.API.Client.Core
{
    /// <summary>Base cho tất cả client - build URL, JWT, deserialize ResponseData</summary>
    public abstract class ApiClientBase
    {
        protected static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        protected HttpClient HttpClient { get; }
        protected ApiClientOptions Options { get; }
        protected IApiTokenProvider TokenProvider { get; }

        protected ApiClientBase(
            HttpClient httpClient,
            IOptions<ApiClientOptions> options,
            IApiTokenProvider tokenProvider)
        {
            HttpClient = httpClient;
            Options = options.Value;
            TokenProvider = tokenProvider;
        }

        protected Uri BuildUri(string controller, string route, IDictionary<string, string?>? query = null)
        {
            var path = $"api/v{Options.ApiVersion}/{controller}/{route}";
            if (query != null && query.Count > 0)
            {
                var qs = string.Join("&",
                    query.Where(x => !string.IsNullOrWhiteSpace(x.Value))
                         .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
                if (!string.IsNullOrEmpty(qs))
                    path += "?" + qs;
            }
            return new Uri(HttpClient.BaseAddress!, path);
        }

        protected async Task<ResponseData<T>> SendAsync<T>(
            HttpMethod method,
            Uri uri,
            object? body = null,
            CancellationToken cancellationToken = default) where T : class
        {
            using var request = new HttpRequestMessage(method, uri);
            if (!string.IsNullOrWhiteSpace(TokenProvider.Token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TokenProvider.Token);

            if (body != null)
                request.Content = JsonContent.Create(body, options: JsonOptions);

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(content))
                return new ResponseData<T>(response.IsSuccessStatusCode, default);

            var trimmed = content.Trim();
            if (!trimmed.StartsWith('{'))
                return ToNonJsonResponse<T>(response, trimmed);

            try
            {
                return JsonSerializer.Deserialize<ResponseData<T>>(trimmed, JsonOptions)
                    ?? new ResponseData<T>("Không thể đọc phản hồi từ API");
            }
            catch (JsonException)
            {
                return ToNonJsonResponse<T>(response, trimmed);
            }
        }

        private static ResponseData<T> ToNonJsonResponse<T>(HttpResponseMessage response, string message) where T : class
            => new()
            {
                Success = false,
                Message = message,
                StatusCode = (int)response.StatusCode
            };

        protected static Dictionary<string, string?> BuildSearchQuery(BaseSearch search)
        {
            var query = new Dictionary<string, string?>
            {
                ["pageIndex"] = search.PageIndex.ToString(),
                ["pageSize"] = search.PageSize.ToString()
            };
            if (search.Id > 0) query["id"] = search.Id.ToString();
            if (search.Status > 0) query["status"] = search.Status.ToString();
            if (!string.IsNullOrWhiteSpace(search.Keyword)) query["keyword"] = search.Keyword;
            return query;
        }
    }
}
