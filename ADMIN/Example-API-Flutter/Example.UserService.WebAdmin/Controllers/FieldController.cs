using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.Field;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class FieldController : Controller
    {
        private readonly IFieldService _service;

        public FieldController(IFieldService service) => _service = service;

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            ViewData["Menu"] = "Field";
            ViewData["PageTitle"] = "Lĩnh vực";
            ViewData["Breadcrumb"] = "Lĩnh vực";
            ViewData["Title"] = "Lĩnh vực";

            var (items, total, error) = await _service.GetListPagingAsync(pageIndex);
            return View(new FieldIndexViewModel { Items = items, TotalItems = total, PageIndex = pageIndex, ErrorMessage = error });
        }

        public IActionResult Create()
        {
            SetFormViewData("Thêm lĩnh vực", "Thêm mới");
            return View("Form", _service.BuildCreateForm());
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.FIELD_CREATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FieldFormViewModel model)
        {
            _service.PopulateFormOptions(model);
            if (!ModelState.IsValid) { SetFormViewData("Thêm lĩnh vực", "Thêm mới"); return View("Form", model); }

            var result = await _service.InsertAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Thêm lĩnh vực", "Thêm mới"); return View("Form", model); }

            TempData["SuccessMessage"] = "Thêm lĩnh vực thành công.";
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

            SetFormViewData("Sửa lĩnh vực", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.FIELD_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FieldFormViewModel model)
        {
            _service.PopulateFormOptions(model);
            if (!ModelState.IsValid) { SetFormViewData("Sửa lĩnh vực", "Sửa"); return View("Form", model); }

            var result = await _service.UpdateAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Sửa lĩnh vực", "Sửa"); return View("Form", model); }

            TempData["SuccessMessage"] = "Cập nhật lĩnh vực thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.FIELD_DELETE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa lĩnh vực thành công." : result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.FIELD_UPDATE)]
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
            ViewData["Menu"] = "Field";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }
    }
}
