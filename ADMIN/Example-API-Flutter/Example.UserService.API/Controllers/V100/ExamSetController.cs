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
    public class ExamSetController : BaseController
    {
        private readonly IExamSetService _examSetService;

        public ExamSetController(IExamSetService examSetService)
            => _examSetService = examSetService;

        [HttpGet]
        [Route("get-list-examSet")]
        public async Task<IActionResult> GetList([FromQuery] ExamSetSearchModel model)
            => Ok(await _examSetService.GetListPaging(model));

        [HttpGet]
        [Route("get-by-id")]
        public async Task<IActionResult> GetById(long id)
            => Ok(await _examSetService.GetById(id));
    }
}
