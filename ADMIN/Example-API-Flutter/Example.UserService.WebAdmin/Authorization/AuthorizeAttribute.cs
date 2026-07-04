using Example.Common.Enums;
using Example.Common.Models;
using Example.Common.Models.AppSetting;
using Example.Common.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Example.UserService.WebAdmin.Authorization
{
    /// <summary>
    /// Phân quyền MVC — logic giống Example.Common.Authorization.AuthorizeAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _permissions;

        public AuthorizeAttribute(params string[] permissions)
        {
            _permissions = permissions;
        }

        public AuthorizeAttribute(PermissionCodeEnum permission)
            : this(permission.ToString())
        {
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            var user = context.HttpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var isCheckToken = AppSettings.Instance.Get<AuthSettingModel>("AuthSetting")?.IsCheckToken ?? true;
            if (!isCheckToken)
                return;

            if (_permissions == null || _permissions.Length == 0)
                return;

            var userPermissions = user.Claims
                .Where(x => x.Type == "permission")
                .Select(x => x.Value)
                .ToList();

            if (userPermissions.Contains(PermissionCodeEnum.SUPER_ADMIN.ToString()))
                return;

            var hasPermission = _permissions.Any(p => userPermissions.Contains(p));
            if (!hasPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
