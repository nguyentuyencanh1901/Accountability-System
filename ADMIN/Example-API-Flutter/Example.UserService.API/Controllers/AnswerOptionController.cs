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
    public class AnswerOptionController : BaseController
    {
        private readonly ILogger<AnswerOptionController> _logger;
        private readonly IAnswerOptionService _answerOptionService;

        public AnswerOptionController(ILogger<AnswerOptionController> logger, IAnswerOptionService answerOptionService)
        {
            _logger = logger;
            _answerOptionService = answerOptionService;
        }

        [HttpGet]
        [Route("get-list-answerOption")]
        [AuthCustom(PermissionCodeEnum.ANSWEROPTION_LIST)]
        public async Task<IActionResult> GetList([FromQuery] AnswerOptionSearchModel model)
        {
            var result = await _answerOptionService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.ANSWEROPTION_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _answerOptionService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.ANSWEROPTION_CREATE)]
        public async Task<IActionResult> AddAsync(AnswerOptionSaveModel model)
        {
            var result = await _answerOptionService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.ANSWEROPTION_UPDATE)]
        public async Task<IActionResult> UpdateAsync(AnswerOptionSaveModel model)
        {
            var result = await _answerOptionService.Update(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.ANSWEROPTION_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _answerOptionService.Delete(id);
            return Ok(result);
        }
    }
}
