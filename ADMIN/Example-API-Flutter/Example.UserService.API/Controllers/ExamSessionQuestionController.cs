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
    public class ExamSessionQuestionController : BaseController
    {
        private readonly ILogger<ExamSessionQuestionController> _logger;
        private readonly IExamSessionQuestionService _examSessionQuestionService;

        public ExamSessionQuestionController(ILogger<ExamSessionQuestionController> logger, IExamSessionQuestionService examSessionQuestionService)
        {
            _logger = logger;
            _examSessionQuestionService = examSessionQuestionService;
        }

        [HttpGet]
        [Route("get-list-examSessionQuestion")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONQUESTION_LIST)]
        public async Task<IActionResult> GetList([FromQuery] ExamSessionQuestionSearchModel model)
        {
            var result = await _examSessionQuestionService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONQUESTION_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _examSessionQuestionService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONQUESTION_CREATE)]
        public async Task<IActionResult> AddAsync(ExamSessionQuestionSaveModel model)
        {
            var result = await _examSessionQuestionService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONQUESTION_UPDATE)]
        public async Task<IActionResult> UpdateAsync(ExamSessionQuestionSaveModel model)
        {
            var result = await _examSessionQuestionService.Update(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONQUESTION_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _examSessionQuestionService.Delete(id);
            return Ok(result);
        }
    }
}
