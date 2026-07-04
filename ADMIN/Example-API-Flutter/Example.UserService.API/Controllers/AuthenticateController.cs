using Asp.Versioning;
using Example.Common.Base;
using Example.Common.Const;
using Example.Common.Enums;
using Example.Common.Models;
using Example.UserService.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthRequired = Example.Common.Authorization.AuthorizeAttribute;

namespace Example.UserService.API.Controllers
{
    [ApiVersion(StaticVariable.ApiVersionV1)]
    public class AuthenticateController : BaseController
    {
        private readonly IAuthenticateService _authenticateService;
        public AuthenticateController(IAuthenticateService authenticateService)
        {
            _authenticateService = authenticateService;
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var result = await _authenticateService.Register(model);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var result = await _authenticateService.Login(model);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login-app")]
        public async Task<IActionResult> LoginApp(LoginModel model)
        {
            var result = await _authenticateService.LoginApp(model);
            return Ok(result);
        }

        [AuthRequired]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileModel model)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Ok(new ResponseData<object>(ErrorCodeAPI.InvalidInput));
            }

            var result = await _authenticateService.UpdateProfile(userId, model);
            return Ok(result);
        }

        [AuthRequired]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Ok(new ResponseData<object>(ErrorCodeAPI.InvalidInput));
            }

            var result = await _authenticateService.ChangePassword(userId, model);
            return Ok(result);
        }

        private long GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && long.TryParse(claim.Value, out var userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
