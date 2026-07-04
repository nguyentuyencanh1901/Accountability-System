using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.ExamPeriodAssignment;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class ExamPeriodAssignmentController : Controller
    {
        private readonly IExamPeriodAssignmentService _service;

        public ExamPeriodAssignmentController(IExamPeriodAssignmentService service) => _service = service;

        public async Task<IActionResult> Index(long? examPeriodId, int pageIndex = 1)
        {
            ViewData["Menu"] = "ExamPeriodAssignment";
            ViewData["PageTitle"] = "Phân công thi";
            ViewData["Breadcrumb"] = "Phân công thi";
            ViewData["Title"] = "Phân công thi";

            var (items, total, examPeriodName, error) = await _service.GetListPagingAsync(examPeriodId, pageIndex);

            return View(new ExamPeriodAssignmentIndexViewModel
            {
                Items = items,
                TotalItems = total,
                PageIndex = pageIndex,
                ExamPeriodId = examPeriodId,
                ExamPeriodName = examPeriodName,
                ErrorMessage = error
            });
        }

        public async Task<IActionResult> Create(long? examPeriodId)
        {
            SetFormViewData("Thêm phân công", "Thêm mới");
            return View("Form", await _service.BuildCreateFormAsync(examPeriodId));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMPERIODASSIGNMENT_CREATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamPeriodAssignmentFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            foreach (var (key, message) in _service.ValidateCreate(model))
                ModelState.AddModelError(key, message);

            if (!ModelState.IsValid) { SetFormViewData("Thêm phân công", "Thêm mới"); return View("Form", model); }

            var result = await _service.InsertBulkAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Thêm phân công", "Thêm mới"); return View("Form", model); }

            TempData["SuccessMessage"] = "Thêm phân công thành công.";
            return RedirectToAction(nameof(Index), new { examPeriodId = model.ExamPeriodId });
        }

        public async Task<IActionResult> Edit(long id)
        {
            var (formModel, errorMessage) = await _service.GetEditFormAsync(id);
            if (formModel == null)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            SetFormViewData("Sửa phân công", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMPERIODASSIGNMENT_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExamPeriodAssignmentFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            foreach (var (key, message) in _service.ValidateEdit(model))
                ModelState.AddModelError(key, message);

            if (!ModelState.IsValid) { SetFormViewData("Sửa phân công", "Sửa"); return View("Form", model); }

            var result = await _service.UpdateAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Sửa phân công", "Sửa"); return View("Form", model); }

            TempData["SuccessMessage"] = "Cập nhật phân công thành công.";
            return RedirectToAction(nameof(Index), new { examPeriodId = model.ExamPeriodId });
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMPERIODASSIGNMENT_DELETE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id, long? examPeriodId)
        {
            var result = await _service.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa phân công thành công." : result.Message;
            return RedirectToAction(nameof(Index), new { examPeriodId });
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMPERIODASSIGNMENT_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, int status, long? examPeriodId, int pageIndex = 1)
        {
            var result = await _service.UpdateStatusAsync(id, status);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Cập nhật trạng thái thành công." : (result.Message ?? "Không thể cập nhật trạng thái.");
            return RedirectToAction(nameof(Index), new { examPeriodId, pageIndex });
        }

        private void SetFormViewData(string title, string breadcrumb)
        {
            ViewData["Menu"] = "ExamPeriodAssignment";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }
    }
}
