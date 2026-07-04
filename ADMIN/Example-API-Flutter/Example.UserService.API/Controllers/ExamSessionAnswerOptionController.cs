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
    public class ExamSessionAnswerOptionController : BaseController
    {
        private readonly ILogger<ExamSessionAnswerOptionController> _logger;
        private readonly IExamSessionAnswerOptionService _examSessionAnswerOptionService;

        public ExamSessionAnswerOptionController(ILogger<ExamSessionAnswerOptionController> logger, IExamSessionAnswerOptionService examSessionAnswerOptionService)
        {
            _logger = logger;
            _examSessionAnswerOptionService = examSessionAnswerOptionService;
        }

        [HttpGet]
        [Route("get-list-examSessionAnswerOption")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWEROPTION_LIST)]
        public async Task<IActionResult> GetList([FromQuery] ExamSessionAnswerOptionSearchModel model)
        {
            var result = await _examSessionAnswerOptionService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWEROPTION_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _examSessionAnswerOptionService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWEROPTION_CREATE)]
        public async Task<IActionResult> AddAsync(ExamSessionAnswerOptionSaveModel model)
        {
            var result = await _examSessionAnswerOptionService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWEROPTION_UPDATE)]
        public async Task<IActionResult> UpdateAsync(ExamSessionAnswerOptionSaveModel model)
        {
            var result = await _examSessionAnswerOptionService.Update(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWEROPTION_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _examSessionAnswerOptionService.Delete(id);
            return Ok(result);
        }
    }
}
