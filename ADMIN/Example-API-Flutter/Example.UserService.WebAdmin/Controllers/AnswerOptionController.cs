using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.AnswerOption;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class AnswerOptionController : Controller
    {
        private readonly IAnswerOptionService _service;

        public AnswerOptionController(IAnswerOptionService service) => _service = service;

        public async Task<IActionResult> Index(long questionId, int pageIndex = 1)
        {
            if (questionId <= 0)
                return RedirectToAction("Index", "Question");

            var (items, total, fieldId, fieldName, questionContent, error, invalidQuestion) =
                await _service.GetListPagingAsync(questionId, pageIndex);
            if (invalidQuestion)
            {
                TempData["ErrorMessage"] = "Không tìm thấy câu hỏi.";
                return RedirectToAction("Index", "Question");
            }

            ViewData["Menu"] = "Question";
            ViewData["PageTitle"] = "Đáp án";
            ViewData["Breadcrumb"] = "Đáp án";
            ViewData["Title"] = "Đáp án";

            return View(new AnswerOptionIndexViewModel
            {
                Items = items,
                TotalItems = total,
                PageIndex = pageIndex,
                QuestionId = questionId,
                FieldId = fieldId,
                FieldName = fieldName,
                QuestionContent = questionContent,
                ErrorMessage = error
            });
        }

        public async Task<IActionResult> Create(long questionId)
        {
            if (questionId <= 0)
                return RedirectToAction("Index", "Question");

            var (formModel, errorMessage, invalidQuestion) = await _service.BuildCreateFormAsync(questionId);
            if (invalidQuestion || formModel == null)
            {
                TempData["ErrorMessage"] = errorMessage ?? "Không tìm thấy câu hỏi.";
                return RedirectToAction("Index", "Question");
            }

            SetFormViewData("Thêm đáp án", "Thêm mới");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.ANSWEROPTION_CREATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnswerOptionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetFormViewData("Thêm đáp án", "Thêm mới");
                return View("Form", model);
            }

            var result = await _service.InsertAsync(model);
            if (!result.Success)
            {
                model.ErrorMessage = result.Message;
                SetFormViewData("Thêm đáp án", "Thêm mới");
                return View("Form", model);
            }

            TempData["SuccessMessage"] = "Thêm đáp án thành công.";
            return RedirectToAction(nameof(Index), new { questionId = model.QuestionId });
        }

        public async Task<IActionResult> Edit(long id)
        {
            var (formModel, errorMessage) = await _service.GetEditFormAsync(id);
            if (formModel == null)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("Index", "Question");
            }

            SetFormViewData("Sửa đáp án", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.ANSWEROPTION_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AnswerOptionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetFormViewData("Sửa đáp án", "Sửa");
                return View("Form", model);
            }

            var result = await _service.UpdateAsync(model);
            if (!result.Success)
            {
                model.ErrorMessage = result.Message;
                SetFormViewData("Sửa đáp án", "Sửa");
                return View("Form", model);
            }

            TempData["SuccessMessage"] = "Cập nhật đáp án thành công.";
            return RedirectToAction(nameof(Index), new { questionId = model.QuestionId });
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.ANSWEROPTION_DELETE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id, long questionId)
        {
            var result = await _service.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa đáp án thành công." : result.Message;
            return RedirectToAction(nameof(Index), new { questionId = questionId > 0 ? questionId : 0 });
        }

        private void SetFormViewData(string title, string breadcrumb)
        {
            ViewData["Menu"] = "Question";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }
    }
}
