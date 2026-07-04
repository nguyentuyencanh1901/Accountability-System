using Example.API.Client.Core;
using Example.UserService.WebAdmin.Const;

namespace Example.UserService.WebAdmin.Middleware
{
    /// <summary>
    /// Đồng bộ JWT từ Session vào ApiTokenProvider trước mỗi request
    /// </summary>
    public class SessionTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IApiTokenProvider tokenProvider)
        {
            var token = context.Session.GetString(SessionKeys.Token);
            tokenProvider.Token = token;
            await _next(context);
        }
    }
}
