using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using Example.Common.Utilities.Helper;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.Question;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _service;

        public QuestionController(IQuestionService service) => _service = service;

        public async Task<IActionResult> Index(
            long? fieldId,
            int? questionType,
            int? difficultyLevel,
            string? keyword,
            int pageIndex = 1)
        {
            ViewData["Menu"] = "Question";
            ViewData["PageTitle"] = "Câu hỏi";
            ViewData["Breadcrumb"] = "Câu hỏi";
            ViewData["Title"] = "Quản lý câu hỏi";

            var model = await _service.GetListPagingAsync(new QuestionIndexViewModel
            {
                PageIndex = pageIndex,
                FieldIdFilter = fieldId,
                QuestionTypeFilter = questionType,
                DifficultyLevelFilter = difficultyLevel,
                Keyword = keyword
            });
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            SetFormViewData("Thêm câu hỏi", "Thêm mới");
            return View("Form", await _service.BuildCreateFormAsync());
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.QUESTION_CREATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionFormViewModel model)
        {
            _service.PopulateFormOptions(model);
            await _service.PopulateDropdownOptionsAsync(model);
            ValidateImageFile(model);
            if (!ModelState.IsValid)
            {
                _service.ApplyImagePreview(model);
                SetFormViewData("Thêm câu hỏi", "Thêm mới");
                return View("Form", model);
            }

            var result = await _service.InsertAsync(model);
            if (!result.Success)
            {
                model.ErrorMessage = result.Message;
                _service.ApplyImagePreview(model);
                SetFormViewData("Thêm câu hỏi", "Thêm mới");
                return View("Form", model);
            }

            TempData["SuccessMessage"] = "Thêm câu hỏi thành công.";
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

            SetFormViewData("Sửa câu hỏi", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.QUESTION_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionFormViewModel model)
        {
            _service.PopulateFormOptions(model);
            await _service.PopulateDropdownOptionsAsync(model);
            ValidateImageFile(model);
            if (!ModelState.IsValid)
            {
                _service.ApplyImagePreview(model);
                SetFormViewData("Sửa câu hỏi", "Sửa");
                return View("Form", model);
            }

            var result = await _service.UpdateAsync(model);
            if (!result.Success)
            {
                model.ErrorMessage = result.Message;
                _service.ApplyImagePreview(model);
                SetFormViewData("Sửa câu hỏi", "Sửa");
                return View("Form", model);
            }

            TempData["SuccessMessage"] = "Cập nhật câu hỏi thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.QUESTION_DELETE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa câu hỏi thành công." : result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.QUESTION_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, int status,
            long? fieldId, int? questionType, int? difficultyLevel, string? keyword, int pageIndex = 1)
        {
            var result = await _service.UpdateStatusAsync(id, status);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Cập nhật trạng thái thành công." : (result.Message ?? "Không thể cập nhật trạng thái.");
            return RedirectToAction(nameof(Index), new { fieldId, questionType, difficultyLevel, keyword, pageIndex });
        }

        private void SetFormViewData(string title, string breadcrumb)
        {
            ViewData["Menu"] = "Question";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }

        private void ValidateImageFile(QuestionFormViewModel model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                return;
            }

            var validation = EntityImageHelper.ValidateFile(model.ImageFile);
            if (!validation.IsValid)
            {
                ModelState.AddModelError(nameof(model.ImageFile), validation.ErrorMessage ?? "File ảnh không hợp lệ");
            }
        }
    }
}
