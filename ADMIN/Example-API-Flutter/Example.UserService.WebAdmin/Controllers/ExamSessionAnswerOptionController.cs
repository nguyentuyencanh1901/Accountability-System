using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.ExamSessionAnswerOption;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class ExamSessionAnswerOptionController : Controller
    {
        private readonly IExamSessionAnswerOptionService _service;

        public ExamSessionAnswerOptionController(IExamSessionAnswerOptionService service) => _service = service;

        public async Task<IActionResult> Index(int pageIndex = 1, long? examSessionAnswerId = null, long? answerOptionId = null)
        {
            ViewData["Menu"] = "ExamSessionAnswerOption";
            ViewData["PageTitle"] = "Đáp án đã chọn";
            ViewData["Breadcrumb"] = "Đáp án đã chọn";
            ViewData["Title"] = "Đáp án đã chọn";

            var (items, total, error, examSessionAnswerFilterOptions, answerOptionFilterOptions) =
                await _service.GetListPagingAsync(pageIndex, examSessionAnswerId, answerOptionId);

            return View(new ExamSessionAnswerOptionIndexViewModel
            {
                Items = items,
                TotalItems = total,
                PageIndex = pageIndex,
                ExamSessionAnswerIdFilter = examSessionAnswerId,
                AnswerOptionIdFilter = answerOptionId,
                ExamSessionAnswerFilterOptions = examSessionAnswerFilterOptions,
                AnswerOptionFilterOptions = answerOptionFilterOptions,
                ErrorMessage = error
            });
        }

        public async Task<IActionResult> Create()
        {
            SetFormViewData("Thêm đáp án đã chọn", "Thêm mới");
            return View("Form", await _service.BuildCreateFormAsync());
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWEROPTION_CREATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamSessionAnswerOptionFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Thêm đáp án đã chọn", "Thêm mới"); return View("Form", model); }

            var result = await _service.InsertAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Thêm đáp án đã chọn", "Thêm mới"); return View("Form", model); }

            TempData["SuccessMessage"] = "Thêm đáp án đã chọn thành công.";
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

            SetFormViewData("Sửa đáp án đã chọn", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWEROPTION_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExamSessionAnswerOptionFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Sửa đáp án đã chọn", "Sửa"); return View("Form", model); }

            var result = await _service.UpdateAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Sửa đáp án đã chọn", "Sửa"); return View("Form", model); }

            TempData["SuccessMessage"] = "Cập nhật đáp án đã chọn thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONANSWEROPTION_DELETE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa đáp án đã chọn thành công." : result.Message;
            return RedirectToAction(nameof(Index));
        }

        private void SetFormViewData(string title, string breadcrumb)
        {
            ViewData["Menu"] = "ExamSessionAnswerOption";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }
    }
}
