using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.ExamPeriod;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class ExamPeriodController : Controller
    {
        private readonly IExamPeriodService _service;

        public ExamPeriodController(IExamPeriodService service) => _service = service;

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            ViewData["Menu"] = "ExamPeriod";
            ViewData["PageTitle"] = "Kỳ thi";
            ViewData["Breadcrumb"] = "Kỳ thi";
            ViewData["Title"] = "Kỳ thi";

            var (items, total, error) = await _service.GetListPagingAsync(pageIndex);
            return View(new ExamPeriodIndexViewModel { Items = items, TotalItems = total, PageIndex = pageIndex, ErrorMessage = error });
        }

        public async Task<IActionResult> Details(long id, int? status, int? examType)
        {
            ViewData["Menu"] = "ExamPeriod";
            ViewData["PageTitle"] = "Theo dõi kỳ thi";
            ViewData["Breadcrumb"] = "Theo dõi";
            ViewData["Title"] = "Theo dõi kỳ thi";

            var (model, error) = await _service.GetMonitoringAsync(id, status, examType);
            if (model == null)
            {
                TempData["ErrorMessage"] = error;
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            SetFormViewData("Thêm kỳ thi", "Thêm mới");
            return View("Form", await _service.BuildCreateFormAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GetExamSetsByType(int type, [FromQuery] List<long>? selectedIds = null)
        {
            var list = await _service.GetExamSetOptionsByTypeAsync(type, selectedIds);
            return Json(list.Select(x => new { id = x.Id, name = x.Name, selected = x.Selected }));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMPERIOD_CREATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamPeriodFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            foreach (var (key, message) in _service.ValidateSelections(model))
                ModelState.AddModelError(key, message);
            if (!ModelState.IsValid) { SetFormViewData("Thêm kỳ thi", "Thêm mới"); return View("Form", model); }

            var result = await _service.InsertAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Thêm kỳ thi", "Thêm mới"); return View("Form", model); }

            TempData["SuccessMessage"] = "Thêm kỳ thi thành công.";
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

            SetFormViewData("Sửa kỳ thi", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMPERIOD_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExamPeriodFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            foreach (var (key, message) in _service.ValidateSelections(model))
                ModelState.AddModelError(key, message);
            if (!ModelState.IsValid) { SetFormViewData("Sửa kỳ thi", "Sửa"); return View("Form", model); }

            var result = await _service.UpdateAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Sửa kỳ thi", "Sửa"); return View("Form", model); }

            TempData["SuccessMessage"] = "Cập nhật kỳ thi thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMPERIOD_DELETE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa kỳ thi thành công." : result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMPERIOD_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, int status, int pageIndex = 1)
        {
            var result = await _service.UpdateStatusAsync(id, status);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Cập nhật trạng thái thành công." : (result.Message ?? "Không thể cập nhật trạng thái.");
            return RedirectToAction(nameof(Index), new { pageIndex });
        }

        private void SetFormViewData(string title, string breadcrumb)
        {
            ViewData["Menu"] = "ExamPeriod";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }
    }
}
