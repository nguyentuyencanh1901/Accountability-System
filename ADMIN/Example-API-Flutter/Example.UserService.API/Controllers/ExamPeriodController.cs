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
    public class ExamPeriodController : BaseController
    {
        private readonly ILogger<ExamPeriodController> _logger;
        private readonly IExamPeriodService _examPeriodService;

        public ExamPeriodController(ILogger<ExamPeriodController> logger, IExamPeriodService examPeriodService)
        {
            _logger = logger;
            _examPeriodService = examPeriodService;
        }

        [HttpGet]
        [Route("get-list-examPeriod")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIOD_LIST)]
        public async Task<IActionResult> GetList([FromQuery] ExamPeriodSearchModel model)
        {
            var result = await _examPeriodService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIOD_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _examPeriodService.GetById(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-monitoring")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIOD_LIST)]
        public async Task<IActionResult> GetMonitoring(long id)
        {
            var result = await _examPeriodService.GetMonitoring(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIOD_CREATE)]
        public async Task<IActionResult> AddAsync(ExamPeriodSaveModel model)
        {
            var result = await _examPeriodService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIOD_UPDATE)]
        public async Task<IActionResult> UpdateAsync(ExamPeriodSaveModel model)
        {
            var result = await _examPeriodService.Update(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIOD_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _examPeriodService.Delete(id);
            return Ok(result);
        }
    }
}
