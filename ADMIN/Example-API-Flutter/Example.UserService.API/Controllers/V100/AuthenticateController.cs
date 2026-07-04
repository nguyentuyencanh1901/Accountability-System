using Asp.Versioning;
using Example.Common.Authorization;
using Example.Common.Base;
using Example.Common.Const;
using Example.Common.Enums;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthRequired = Example.Common.Authorization.AuthorizeAttribute;

namespace Example.UserService.API.Controllers.V100
{
    /// <summary>API v100 cho WebApp — đăng nhập/đăng ký không cần quyền admin.</summary>
    [ApiVersion(StaticVariable.ApiVersionV100)]
    public class AuthenticateController : BaseController
    {
        private readonly IAuthenticateService _authenticateService;

        public AuthenticateController(IAuthenticateService authenticateService)
            => _authenticateService = authenticateService;

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
            => Ok(await _authenticateService.Register(model));

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
            => Ok(await _authenticateService.Login(model));

        [AllowAnonymous]
        [HttpPost("login-app")]
        public async Task<IActionResult> LoginApp(LoginModel model)
            => Ok(await _authenticateService.LoginApp(model));

        [AuthRequired]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileModel model)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
                return Ok(new ResponseData<object>(ErrorCodeAPI.InvalidInput));

            return Ok(await _authenticateService.UpdateProfile(userId, model));
        }

        [AuthRequired]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
                return Ok(new ResponseData<object>(ErrorCodeAPI.InvalidInput));

            return Ok(await _authenticateService.ChangePassword(userId, model));
        }

        private long GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && long.TryParse(claim.Value, out var userId) ? userId : 0;
        }
    }
}
