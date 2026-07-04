using Asp.Versioning;
using Example.Common.Authorization;
using Example.Common.Base;
using Example.Common.Const;
using Example.UserService.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.API.Controllers.V100
{
    /// <summary>API v100 cho WebApp — chỉ cần đăng nhập, không kiểm tra quyền chi tiết.</summary>
    [ApiVersion(StaticVariable.ApiVersionV100)]
    [Authorize]
    public class AppUserController : BaseController
    {
        private readonly IAppUserService _appUserService;

        public AppUserController(IAppUserService appUserService)
            => _appUserService = appUserService;

        [HttpGet]
        [Route("get-by-id")]
        public async Task<IActionResult> GetById(long id)
            => Ok(await _appUserService.GetById(id));
    }
}
