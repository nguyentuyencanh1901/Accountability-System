using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.Role;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private readonly IRoleService _service;

        public RoleController(IRoleService service) => _service = service;

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            ViewData["Menu"] = "Role";
            ViewData["PageTitle"] = "Vai trò";
            ViewData["Breadcrumb"] = "Vai trò";
            ViewData["Title"] = "Vai trò";

            var (items, total, error) = await _service.GetListPagingAsync(pageIndex);

            return View(new RoleIndexViewModel { Items = items, TotalItems = total, PageIndex = pageIndex, ErrorMessage = error });
        }

        public async Task<IActionResult> Create()
        {
            SetFormViewData("Thêm vai trò", "Thêm mới");
            return View("Form", await _service.BuildCreateFormAsync());
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.ROLE_CREATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Thêm vai trò", "Thêm mới"); return View("Form", model); }

            var result = await _service.InsertAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Thêm vai trò", "Thêm mới"); return View("Form", model); }

            TempData["SuccessMessage"] = "Thêm vai trò thành công.";
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

            SetFormViewData("Sửa vai trò", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.ROLE_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Sửa vai trò", "Sửa"); return View("Form", model); }

            var result = await _service.UpdateAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Sửa vai trò", "Sửa"); return View("Form", model); }

            TempData["SuccessMessage"] = "Cập nhật vai trò thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.ROLE_DELETE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa vai trò thành công." : result.Message;
            return RedirectToAction(nameof(Index));
        }

        private void SetFormViewData(string title, string breadcrumb)
        {
            ViewData["Menu"] = "Role";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }
    }
}
