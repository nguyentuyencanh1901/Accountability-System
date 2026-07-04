using Asp.Versioning;
using Example.Common.Authorization;
using Example.Common.Base;
using Example.Common.Const;
using Example.Common.Enums;
using Example.UserService.API.Models;
using Example.UserService.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using AuthCustom = Example.Common.Authorization.AuthorizeAttribute;

namespace Example.UserService.API.Controllers
{
    [ApiVersion(StaticVariable.ApiVersionV1)]
    [Authorize]
    public class RoleController : BaseController
    {
        private readonly ILogger<RoleController> _logger;
        private readonly IRoleService _roleService;

        public RoleController(ILogger<RoleController> logger, IRoleService roleService)
        {
            _logger = logger;
            _roleService = roleService;
        }

        [HttpGet]
        [Route("get-list-role")]
        [AuthCustom(PermissionCodeEnum.ROLE_LIST)]
        public async Task<IActionResult> GetList([FromQuery] RoleSearchModel model)
        {
            var result = await _roleService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.ROLE_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _roleService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.ROLE_CREATE)]
        public async Task<IActionResult> AddAsync(RoleSaveModel model)
        {
            var result = await _roleService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.ROLE_UPDATE)]
        public async Task<IActionResult> UpdateAsync(RoleSaveModel model)
        {
            var result = await _roleService.Update(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.ROLE_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _roleService.Delete(id);
            return Ok(result);
        }
    }
}
