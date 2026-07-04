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
    public class ExamSessionController : BaseController
    {
        private readonly ILogger<ExamSessionController> _logger;
        private readonly IExamSessionService _examSessionService;

        public ExamSessionController(ILogger<ExamSessionController> logger, IExamSessionService examSessionService)
        {
            _logger = logger;
            _examSessionService = examSessionService;
        }

        [HttpGet]
        [Route("get-list-examSession")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_LIST)]
        public async Task<IActionResult> GetList([FromQuery] ExamSessionSearchModel model)
        {
            var result = await _examSessionService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-history")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_LIST)]
        public async Task<IActionResult> GetHistory([FromQuery] ExamSessionSearchModel model)
        {
            model.HistoryOnly = true;
            model.Status = null;
            var result = await _examSessionService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _examSessionService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_CREATE)]
        public async Task<IActionResult> AddAsync(ExamSessionSaveModel model)
        {
            var result = await _examSessionService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_UPDATE)]
        public async Task<IActionResult> UpdateAsync(ExamSessionSaveModel model)
        {
            var result = await _examSessionService.Update(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("start-exam")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_UPDATE)]
        public async Task<IActionResult> StartExam(StartExamModel model)
        {
            var result = await _examSessionService.StartExam(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("save-exam-progress")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_UPDATE)]
        public async Task<IActionResult> SaveExamProgress(SubmitExamModel model)
        {
            var result = await _examSessionService.SaveExamProgress(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("submit-exam")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_UPDATE)]
        public async Task<IActionResult> SubmitExam(SubmitExamModel model)
        {
            var result = await _examSessionService.SubmitExam(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("cancel-exam-violation")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_UPDATE)]
        public async Task<IActionResult> CancelExamDueToViolation(SubmitExamModel model)
        {
            var result = await _examSessionService.CancelExamDueToViolation(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _examSessionService.Delete(id);
            return Ok(result);
        }
    }
}
