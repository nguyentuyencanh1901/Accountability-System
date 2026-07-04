
using Example.Common.Base;

using Asp.Versioning;
using Example.Common.Authorization;
using Example.Common.Const;
using Example.Common.Enums;
using AuthCustom = Example.Common.Authorization.AuthorizeAttribute;
using Example.UserService.API.Models;
using Example.UserService.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.API.Controllers
{
    [ApiVersion(StaticVariable.ApiVersionV1)]
    [Authorize]
    public class AppUserController : BaseController
    {
        private readonly ILogger<AppUserController> _logger;
        private readonly IAppUserService _appUserService;

        public AppUserController(ILogger<AppUserController> logger, IAppUserService appUserService)
        {
            _logger = logger;
            _appUserService = appUserService;
        }

        [HttpGet]
        [Route("get-list-appUser")]
        [AuthCustom(PermissionCodeEnum.USER_LIST)]
        public async Task<IActionResult> GetList([FromQuery] AppUserSearchModel model)
        {
            var result = await _appUserService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.USER_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _appUserService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.USER_CREATE)]
        public async Task<IActionResult> AddAsync(AppUserSaveModel model)
        {
            var result = await _appUserService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.USER_UPDATE)]

        public async Task<IActionResult> UpdateAsync(AppUserSaveModel model)
        {
            var result = await _appUserService.Update(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("reset-password")]
        [AuthCustom(PermissionCodeEnum.USER_RESET_PASSWORD)]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            var result = await _appUserService.ResetPassword(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.USER_DELETE)]

        public async Task<IActionResult> Delete(long id)
        {
            var result = await _appUserService.Delete(id);
            return Ok(result);
        }
    }
}
