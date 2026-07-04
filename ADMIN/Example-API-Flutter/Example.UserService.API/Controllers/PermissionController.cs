using Asp.Versioning;
using Example.Common.Authorization;
using Example.Common.Base;
using Example.Common.Const;
using Example.Common.Enums;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using AuthCustom = Example.Common.Authorization.AuthorizeAttribute;

namespace Example.UserService.API.Controllers
{
    [ApiVersion(StaticVariable.ApiVersionV1)]
    [Authorize]
    public class PermissionController : BaseController
    {
        private readonly ILogger<PermissionController> _logger;
        private readonly IPermissionService _permissionService;

        public PermissionController(ILogger<PermissionController> logger, IPermissionService permissionService)
        {
            _logger = logger;
            _permissionService = permissionService;
        }

        [HttpGet]
        [Route("get-list-permission")]
        [AuthCustom(PermissionCodeEnum.PERMISSION_LIST)]
        public async Task<IActionResult> GetList([FromQuery] PermissionSearchModel model)
        {
            var result = await _permissionService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.PERMISSION_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _permissionService.GetById(id);
            return Ok(result);
        }

        /// <summary>Quyền hạn được đồng bộ tự động từ PermissionCodeEnum — không cho thêm thủ công.</summary>
        [HttpPost]
        [Route("add")]
        public IActionResult AddAsync(PermissionSaveModel model) =>
            StatusCode(403, new ResponseData<object>("Quyền hạn được đồng bộ tự động từ enum khi khởi động API."));

        /// <summary>Quyền hạn được đồng bộ tự động từ PermissionCodeEnum — không cho sửa thủ công.</summary>
        [HttpPut]
        [Route("update")]
        public IActionResult UpdateAsync(PermissionSaveModel model) =>
            StatusCode(403, new ResponseData<object>("Quyền hạn được đồng bộ tự động từ enum khi khởi động API."));

        /// <summary>Quyền hạn được đồng bộ tự động từ PermissionCodeEnum — không cho xóa thủ công.</summary>
        [HttpDelete]
        [Route("delete")]
        public IActionResult Delete(long id) =>
            StatusCode(403, new ResponseData<object>("Quyền hạn được đồng bộ tự động từ enum khi khởi động API."));
    }
}
