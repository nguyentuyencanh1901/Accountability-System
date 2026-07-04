using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.ExamSessionAnswer;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class ExamSessionAnswerController : Controller
    {
        private readonly IExamSessionAnswerService _service;

        public ExamSessionAnswerController(IExamSessionAnswerService service) => _service = service;

        public async Task<IActionResult> Index(int pageIndex = 1, long? examSessionId = null, long? questionId = null)
        {
            ViewData["Menu"] = "ExamSessionAnswer";
            ViewData["PageTitle"] = "Câu trả lời";
            ViewData["Breadcrumb"] = "Câu trả lời";
            ViewData["Title"] = "Câu trả lời";

            var (items, total, error, examSessionFilterOptions, questionFilterOptions) =
                await _service.GetListPagingAsync(pageIndex, examSessionId, questionId);

            return View(new ExamSessionAnswerIndexViewModel
            {
                Items = items,
                TotalItems = total,
                PageIndex = pageIndex,
                ExamSessionIdFilter = examSessionId,
                QuestionIdFilter = questionId,
                ExamSessionFilterOptions = examSessionFilterOptions,
                QuestionFilterOptions = questionFilterOptions,
                ErrorMessage = error
            });
        }

        public async Task<IActionResult> Create()
        {
            SetFormViewData("Thêm câu trả lời", "Thêm mới");
            return View("Form", await _service.BuildCreateFormAsync());
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWER_CREATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamSessionAnswerFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Thêm câu trả lời", "Thêm mới"); return View("Form", model); }

            var result = await _service.InsertAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Thêm câu trả lời", "Thêm mới"); return View("Form", model); }

            TempData["SuccessMessage"] = "Thêm câu trả lời thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long id)
        {
            var (formModel, errorMessage) = await _service.GetEditFormAsync(id);
            if (formModel == null)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            SetFormViewData("Sửa câu trả lời", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWER_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExamSessionAnswerFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Sửa câu trả lời", "Sửa"); return View("Form", model); }

            var result = await _service.UpdateAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Sửa câu trả lời", "Sửa"); return View("Form", model); }

            TempData["SuccessMessage"] = "Cập nhật câu trả lời thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWER_DELETE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa câu trả lời thành công." : result.Message;
            return RedirectToAction(nameof(Index));
        }

        private void SetFormViewData(string title, string breadcrumb)
        {
            ViewData["Menu"] = "ExamSessionAnswer";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }
    }
}
