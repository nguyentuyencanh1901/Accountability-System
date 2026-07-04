using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.ExamSessionQuestion;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class ExamSessionQuestionController : Controller
    {
        private readonly IExamSessionQuestionService _service;

        public ExamSessionQuestionController(IExamSessionQuestionService service) => _service = service;

        public async Task<IActionResult> Index(int pageIndex = 1, long? examSessionId = null, long? questionId = null)
        {
            ViewData["Menu"] = "ExamSessionQuestion";
            ViewData["PageTitle"] = "Câu hỏi bài thi";
            ViewData["Breadcrumb"] = "Câu hỏi bài thi";
            ViewData["Title"] = "Câu hỏi bài thi";

            var (items, total, error, examSessionFilterOptions, questionFilterOptions) =
                await _service.GetListPagingAsync(pageIndex, examSessionId, questionId);

            return View(new ExamSessionQuestionIndexViewModel
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
            SetFormViewData("Thêm câu hỏi bài thi", "Thêm mới");
            return View("Form", await _service.BuildCreateFormAsync());
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONQUESTION_CREATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamSessionQuestionFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Thêm câu hỏi bài thi", "Thêm mới"); return View("Form", model); }

            var result = await _service.InsertAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Thêm câu hỏi bài thi", "Thêm mới"); return View("Form", model); }

            TempData["SuccessMessage"] = "Thêm câu hỏi bài thi thành công.";
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

            SetFormViewData("Sửa câu hỏi bài thi", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONQUESTION_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExamSessionQuestionFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Sửa câu hỏi bài thi", "Sửa"); return View("Form", model); }

            var result = await _service.UpdateAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Sửa câu hỏi bài thi", "Sửa"); return View("Form", model); }

            TempData["SuccessMessage"] = "Cập nhật câu hỏi bài thi thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSIONQUESTION_DELETE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa câu hỏi bài thi thành công." : result.Message;
            return RedirectToAction(nameof(Index));
        }

        private void SetFormViewData(string title, string breadcrumb)
        {
            ViewData["Menu"] = "ExamSessionQuestion";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }
    }
}
