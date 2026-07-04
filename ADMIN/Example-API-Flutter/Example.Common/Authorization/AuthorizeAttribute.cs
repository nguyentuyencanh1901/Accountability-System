using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Example.Common.Models;
using System.Net;
using System.Security.Claims;
using Example.Common.Const;
using Example.Common.Utilities;
using Example.Common.Enums;
using Example.Common.Utilities.Helper;

#pragma warning disable S1541, S3776
namespace Example.Common.Authorization
{
    /// <summary>
    /// Kiểm tra JWT + permission trên API. Không truyền permission = chỉ cần đăng nhập.
    /// Claim "permission" chứa SUPER_ADMIN hoặc mã quyền (USER_CREATE, ...).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _permissions;

        /// <summary>
        /// Creates a new instance of <see cref="AuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="permissions">A list of permissions to authorize</param>
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
            {
                throw new ArgumentNullException("context");
            }

            // Cho phép endpoint đánh dấu [AllowAnonymous] (login, register)
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
            {
                return;
            }

            if (!StaticVariable.AuthSetting.IsCheckToken)
            {
                // Bỏ qua check bắt buộc phải có token
                return;
            }


            var user = context.HttpContext.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                RejectRequestNotProduct(context, HttpStatusCode.Unauthorized);
                return;
            }

            // Không truyền permission vào attribute → chỉ cần đăng nhập
            if (_permissions == null || _permissions.Length == 0)
            {
                return;
            }


            var userPermissions = user.Claims
                .Where(x => x.Type == "permission")
                .Select(x => x.Value)
                .ToList();

            // ✅ SUPER ADMIN → bypass mọi kiểm tra permission
            if (userPermissions.Contains(PermissionCodeEnum.SUPER_ADMIN.ToString()))
            {
                return;
            }

            // User phải có ít nhất một permission trong danh sách yêu cầu
            var hasPermission = _permissions.Any(p => userPermissions.Contains(p));

            if (!hasPermission)
            {
                RejectRequestNotProduct(context, HttpStatusCode.Unauthorized);
                return;
            }

            // ✅ ĐỦ QUYỀN → CHO ĐI
            return;

        }

        private void RejectRequestNotProduct(AuthorizationFilterContext filterContext, HttpStatusCode statusCode)
        {
            filterContext.HttpContext.Response.StatusCode = (int)statusCode;

            string message = statusCode == HttpStatusCode.Forbidden
                ? "Bạn chưa được cấp quyền với chức năng này."
                : "Bạn chưa đăng nhập."
                ;

            ResponseData<object> responseData = new ResponseData<object>
            {
                Success = false,
                StatusCode = (int)statusCode,
                Message = message
            };

            filterContext.Result = new JsonResult(responseData);
        }
    }
}
#pragma warning restore S1541, S3776