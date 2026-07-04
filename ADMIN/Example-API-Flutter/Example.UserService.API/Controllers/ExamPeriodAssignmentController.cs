using Asp.Versioning;
using Example.Common.Authorization;
using Example.Common.Base;
using Example.Common.Const;
using Example.Common.Enums;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthCustom = Example.Common.Authorization.AuthorizeAttribute;

namespace Example.UserService.API.Controllers
{
    [ApiVersion(StaticVariable.ApiVersionV1)]
    [Authorize]
    public class ExamPeriodAssignmentController : BaseController
    {
        private readonly ILogger<ExamPeriodAssignmentController> _logger;
        private readonly IExamPeriodAssignmentService _examPeriodAssignmentService;

        public ExamPeriodAssignmentController(
            ILogger<ExamPeriodAssignmentController> logger,
            IExamPeriodAssignmentService examPeriodAssignmentService)
        {
            _logger = logger;
            _examPeriodAssignmentService = examPeriodAssignmentService;
        }

        [HttpGet]
        [Route("get-list-examPeriodAssignment")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIODASSIGNMENT_LIST)]
        public async Task<IActionResult> GetList([FromQuery] ExamPeriodAssignmentSearchModel model)
        {
            var result = await _examPeriodAssignmentService.GetListPaging(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-id")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIODASSIGNMENT_LIST)]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _examPeriodAssignmentService.GetById(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-my-assignments")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIODASSIGNMENT_LIST)]
        public async Task<IActionResult> GetMyAssignments()
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Ok(new ResponseData<IEnumerable<ExamPeriodAssignmentModel>>(ErrorCodeAPI.InvalidInput));
            }

            var result = await _examPeriodAssignmentService.GetMyAssignments(userId);
            return Ok(result);
        }

        [HttpPost]
        [Route("add-bulk")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIODASSIGNMENT_CREATE)]
        public async Task<IActionResult> AddBulkAsync(ExamPeriodAssignmentBulkSaveModel model)
        {
            var result = await _examPeriodAssignmentService.InsertBulk(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIODASSIGNMENT_CREATE)]
        public async Task<IActionResult> AddAsync(ExamPeriodAssignmentSaveModel model)
        {
            var result = await _examPeriodAssignmentService.Insert(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIODASSIGNMENT_UPDATE)]
        public async Task<IActionResult> UpdateAsync(ExamPeriodAssignmentSaveModel model)
        {
            var result = await _examPeriodAssignmentService.Update(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [AuthCustom(PermissionCodeEnum.EXAMPERIODASSIGNMENT_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _examPeriodAssignmentService.Delete(id);
            return Ok(result);
        }

        private long GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && long.TryParse(claim.Value, out var userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
