using Example.UserService.WebApp.Const;

namespace Example.UserService.WebApp.Helpers
{
    public static class SessionHelper
    {
        public static long GetUserId(HttpContext httpContext)
        {
            var raw = httpContext.Session.GetString(SessionKeys.UserId);
            return long.TryParse(raw, out var userId) ? userId : 0;
        }
    }
}
