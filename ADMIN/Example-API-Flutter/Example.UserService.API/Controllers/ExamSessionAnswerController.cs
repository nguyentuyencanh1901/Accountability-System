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
    public class ExamSessionAnswerController : BaseController
    {
        private readonly ILogger<ExamSessionAnswerController> _logger;
        private readonly IExamSessionAnswerService _examSessionAnswerService;

        public ExamSessionAnswerController(ILogger<ExamSessionAnswerController> logger, IExamSessionAnswerService examSessionAnswerService)
        {
            _logger = logger;
            _examSessionAnswerService = examSessionAnswerService;
        }

        [HttpGet]
        [Route("get-list-examSessionAnswer")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWER_LIST)]
        public async Task<IActionResult> GetList([FromQuery] ExamSessionAnswerSearchModel model)
        {
            var result = await _examSessionAnswerService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWER_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _examSessionAnswerService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWER_CREATE)]
        public async Task<IActionResult> AddAsync(ExamSessionAnswerSaveModel model)
        {
            var result = await _examSessionAnswerService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWER_UPDATE)]
        public async Task<IActionResult> UpdateAsync(ExamSessionAnswerSaveModel model)
        {
            var result = await _examSessionAnswerService.Update(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWER_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _examSessionAnswerService.Delete(id);
            return Ok(result);
        }
    }
}
