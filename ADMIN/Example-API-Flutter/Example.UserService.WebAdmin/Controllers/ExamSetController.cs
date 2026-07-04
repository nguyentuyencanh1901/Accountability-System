using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.ExamSet;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class ExamSetController : Controller
    {
        private readonly IExamSetService _service;

        public ExamSetController(IExamSetService service) => _service = service;

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            ViewData["Menu"] = "ExamSet";
            ViewData["PageTitle"] = "Bộ đề thi";
            ViewData["Breadcrumb"] = "Bộ đề thi";
            ViewData["Title"] = "Bộ đề thi";

            var (items, total, error) = await _service.GetListPagingAsync(pageIndex);
            return View(new ExamSetIndexViewModel { Items = items, TotalItems = total, PageIndex = pageIndex, ErrorMessage = error });
        }

        public async Task<IActionResult> Create()
        {
            SetFormViewData("Thêm bộ đề", "Thêm mới");
            return View("Form", await _service.BuildCreateFormAsync());
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSET_CREATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamSetFormViewModel model)
        {
            _service.PopulateFormOptions(model);
            await _service.PopulateFieldOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Thêm bộ đề", "Thêm mới"); return View("Form", model); }

            var result = await _service.InsertAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Thêm bộ đề", "Thêm mới"); return View("Form", model); }

            TempData["SuccessMessage"] = "Thêm bộ đề thành công.";
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

            SetFormViewData("Sửa bộ đề", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSET_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExamSetFormViewModel model)
        {
            _service.PopulateFormOptions(model);
            await _service.PopulateFieldOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Sửa bộ đề", "Sửa"); return View("Form", model); }

            var result = await _service.UpdateAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Sửa bộ đề", "Sửa"); return View("Form", model); }

            TempData["SuccessMessage"] = "Cập nhật bộ đề thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSET_DELETE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa bộ đề thành công." : result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSET_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, int status, int pageIndex = 1)
        {
            var result = await _service.UpdateStatusAsync(id, status);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Cập nhật trạng thái thành công." : (result.Message ?? "Không thể cập nhật trạng thái.");
            return RedirectToAction(nameof(Index), new { pageIndex });
        }

        public async Task<IActionResult> Takers(long id, int pageIndex = 1)
        {
            var (examSet, items, total, error) = await _service.GetExamTakersAsync(id, pageIndex);
            if (examSet == null)
            {
                TempData["ErrorMessage"] = error ?? "Không tìm thấy bộ đề.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Menu"] = "ExamSet";
            ViewData["PageTitle"] = "Kết quả thi theo bộ đề";
            ViewData["Breadcrumb"] = "Kết quả thi";
            ViewData["Title"] = "Kết quả thi";

            return View(new ExamSetTakersViewModel
            {
                ExamSetId = examSet.Id,
                ExamSetName = examSet.Name,
                Items = items,
                TotalItems = total,
                PageIndex = pageIndex,
                PageSize = 15,
                ErrorMessage = error
            });
        }

        private void SetFormViewData(string title, string breadcrumb)
        {
            ViewData["Menu"] = "ExamSet";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }
    }
}
