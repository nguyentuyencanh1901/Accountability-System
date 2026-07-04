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
    public class ExamSetController : BaseController
    {
        private readonly ILogger<ExamSetController> _logger;
        private readonly IExamSetService _examSetService;

        public ExamSetController(ILogger<ExamSetController> logger, IExamSetService examSetService)
        {
            _logger = logger;
            _examSetService = examSetService;
        }

        [HttpGet]
        [Route("get-list-examSet")]
        [AuthCustom(PermissionCodeEnum.EXAMSET_LIST)]
        public async Task<IActionResult> GetList([FromQuery] ExamSetSearchModel model)
        {
            var result = await _examSetService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.EXAMSET_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _examSetService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.EXAMSET_CREATE)]
        public async Task<IActionResult> AddAsync(ExamSetSaveModel model)
        {
            var result = await _examSetService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.EXAMSET_UPDATE)]
        public async Task<IActionResult> UpdateAsync(ExamSetSaveModel model)
        {
            var result = await _examSetService.Update(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.EXAMSET_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _examSetService.Delete(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-exam-takers")]
        [AuthCustom(PermissionCodeEnum.EXAMSET_LIST)]
        public async Task<IActionResult> GetExamTakers([FromQuery] ExamSetTakerSearchModel model)
        {
            var result = await _examSetService.GetExamTakers(model);
            return Ok(result);
        }
    }
}
