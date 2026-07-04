using Example.API.Client.Core;
using Example.UserService.WebApp.Const;

namespace Example.UserService.WebApp.Middleware
{
    /// <summary>Gắn JWT từ session vào ApiTokenProvider để mọi request gọi API đều có token.</summary>
    public class SessionTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionTokenMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, IApiTokenProvider tokenProvider)
        {
            tokenProvider.Token = context.Session.GetString(SessionKeys.Token);
            await _next(context);
        }
    }
}
