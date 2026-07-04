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

namespace Example.UserService.API.Controllers.V100
{
    /// <summary>API v100 cho WebApp — chỉ cần đăng nhập, không kiểm tra quyền chi tiết.</summary>
    [ApiVersion(StaticVariable.ApiVersionV100)]
    [Authorize]
    public class ExamPeriodAssignmentController : BaseController
    {
        private readonly IExamPeriodAssignmentService _examPeriodAssignmentService;

        public ExamPeriodAssignmentController(IExamPeriodAssignmentService examPeriodAssignmentService)
            => _examPeriodAssignmentService = examPeriodAssignmentService;

        [HttpGet]
        [Route("get-my-assignments")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
                return Ok(new ResponseData<IEnumerable<ExamPeriodAssignmentModel>>(ErrorCodeAPI.InvalidInput));

            return Ok(await _examPeriodAssignmentService.GetMyAssignments(userId));
        }

        [HttpGet]
        [Route("get-by-id")]
        public async Task<IActionResult> GetById(long id)
            => Ok(await _examPeriodAssignmentService.GetById(id));

        private long GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && long.TryParse(claim.Value, out var userId) ? userId : 0;
        }
    }
}
