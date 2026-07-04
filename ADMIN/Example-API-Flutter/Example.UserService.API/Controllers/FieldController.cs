using Asp.Versioning;
using Example.Common.Authorization;
using Example.Common.Const;
using Example.Common.Enums;
using Example.Common.Base;
using Example.UserService.API.Models;
using Example.UserService.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using AuthCustom = Example.Common.Authorization.AuthorizeAttribute;

namespace Example.UserService.API.Controllers
{
    [ApiVersion(StaticVariable.ApiVersionV1)]
    [Authorize]
    public class FieldController : BaseController    {
        private readonly ILogger<FieldController> _logger;
        private readonly IFieldService _fieldService;

        public FieldController(ILogger<FieldController> logger, IFieldService fieldService)
        {
            _logger = logger;
            _fieldService = fieldService;
        }

        [HttpGet]
        [Route("get-list-field")]
        [AuthCustom(PermissionCodeEnum.FIELD_LIST)]
        public async Task<IActionResult> GetList([FromQuery] FieldSearchModel model)
        {
            var result = await _fieldService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.FIELD_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _fieldService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.FIELD_CREATE)]
        public async Task<IActionResult> AddAsync(FieldSaveModel model)
        {
            var result = await _fieldService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.FIELD_UPDATE)]
        public async Task<IActionResult> UpdateAsync(FieldSaveModel model)
        {
            var result = await _fieldService.Update(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("update-status")]
        [AuthCustom(PermissionCodeEnum.FIELD_UPDATE)]
        public async Task<IActionResult> UpdateStatusAsync(UpdateStatusModel model)
        {
            var result = await _fieldService.UpdateStatus(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.FIELD_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _fieldService.Delete(id);
            return Ok(result);
        }
    }
}
