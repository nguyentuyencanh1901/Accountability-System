using Asp.Versioning;
using Example.Common.Authorization;
using Example.Common.Base;
using Example.Common.Const;
using Example.UserService.API.Models;
using Example.UserService.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.API.Controllers.V100
{
    /// <summary>API v100 cho WebApp — chỉ cần đăng nhập, không kiểm tra quyền chi tiết.</summary>
    [ApiVersion(StaticVariable.ApiVersionV100)]
    [Authorize]
    public class ExamSessionController : BaseController
    {
        private readonly IExamSessionService _examSessionService;

        public ExamSessionController(IExamSessionService examSessionService)
            => _examSessionService = examSessionService;

        [HttpGet]
        [Route("get-list-examSession")]
        public async Task<IActionResult> GetList([FromQuery] ExamSessionSearchModel model)
            => Ok(await _examSessionService.GetListPaging(model));

        [HttpGet]
        [Route("get-history")]
        public async Task<IActionResult> GetHistory([FromQuery] ExamSessionSearchModel model)
        {
            model.HistoryOnly = true;
            model.Status = null;
            return Ok(await _examSessionService.GetListPaging(model));
        }

        [HttpGet]
        [Route("get-by-id")]
        public async Task<IActionResult> GetById(long id)
            => Ok(await _examSessionService.GetById(id));

        [HttpPost]
        [Route("start-exam")]
        public async Task<IActionResult> StartExam(StartExamModel model)
            => Ok(await _examSessionService.StartExam(model));

        [HttpPost]
        [Route("save-exam-progress")]
        public async Task<IActionResult> SaveExamProgress(SubmitExamModel model)
            => Ok(await _examSessionService.SaveExamProgress(model));

        [HttpPost]
        [Route("submit-exam")]
        public async Task<IActionResult> SubmitExam(SubmitExamModel model)
            => Ok(await _examSessionService.SubmitExam(model));

        [HttpPost]
        [Route("cancel-exam-violation")]
        public async Task<IActionResult> CancelExamDueToViolation(SubmitExamModel model)
            => Ok(await _examSessionService.CancelExamDueToViolation(model));
    }
}
