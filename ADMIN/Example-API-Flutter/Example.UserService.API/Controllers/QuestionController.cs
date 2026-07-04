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
    public class QuestionController : BaseController    {
        private readonly ILogger<QuestionController> _logger;
        private readonly IQuestionService _questionService;

        public QuestionController(ILogger<QuestionController> logger, IQuestionService questionService)
        {
            _logger = logger;
            _questionService = questionService;
        }

        [HttpGet]
        [Route("get-list-question")]
        [AuthCustom(PermissionCodeEnum.QUESTION_LIST)]
        public async Task<IActionResult> GetList([FromQuery] QuestionSearchModel model)
        {
            var result = await _questionService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.QUESTION_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _questionService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.QUESTION_CREATE)]
        public async Task<IActionResult> AddAsync(QuestionSaveModel model)
        {
            var result = await _questionService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.QUESTION_UPDATE)]
        public async Task<IActionResult> UpdateAsync(QuestionSaveModel model)
        {
            var result = await _questionService.Update(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("update-status")]
        [AuthCustom(PermissionCodeEnum.QUESTION_UPDATE)]
        public async Task<IActionResult> UpdateStatusAsync(UpdateStatusModel model)
        {
            var result = await _questionService.UpdateStatus(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.QUESTION_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _questionService.Delete(id);
            return Ok(result);
        }
    }
}
