using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.AppUser;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class AppUserController : Controller
    {
        private readonly IAppUserService _service;

        public AppUserController(IAppUserService service) => _service = service;

        public async Task<IActionResult> Index(int pageIndex = 1, int userType = (int)UserTypeEnum.Manager)
        {
            ViewData["Menu"] = "AppUser";
            ViewData["PageTitle"] = "Người dùng";
            ViewData["Breadcrumb"] = "Người dùng";
            ViewData["Title"] = "Người dùng";

            var (items, total, error) = await _service.GetListPagingAsync(pageIndex, userType);

            return View(new AppUserIndexViewModel
            {
                Items = items,
                TotalItems = total,
                PageIndex = pageIndex,
                UserTypeFilter = userType,
                ErrorMessage = error
            });
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Menu"] = "AppUser";
            ViewData["PageTitle"] = "Thêm người dùng";
            ViewData["Breadcrumb"] = "Thêm mới";
            ViewData["Title"] = "Thêm người dùng";
            return View("Form", await _service.BuildCreateFormAsync());
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.USER_CREATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUserFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            foreach (var (key, message) in _service.ValidateCreate(model))
                ModelState.AddModelError(key, message);
            if (!ModelState.IsValid)
            {
                SetFormViewData("Thêm người dùng", "Thêm mới");
                return View("Form", model);
            }

            var result = await _service.InsertAsync(model);

            if (!result.Success)
            {
                model.ErrorMessage = result.Message;
                SetFormViewData("Thêm người dùng", "Thêm mới");
                return View("Form", model);
            }

            TempData["SuccessMessage"] = "Thêm người dùng thành công.";
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

            SetFormViewData("Sửa người dùng", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.USER_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUserFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);

            if (model.IsSuperAdminRestrictedEdit)
            {
                if (string.IsNullOrWhiteSpace(model.FullName))
                    ModelState.AddModelError(nameof(model.FullName), "Vui lòng nhập họ tên");
                if (string.IsNullOrWhiteSpace(model.Email))
                    ModelState.AddModelError(nameof(model.Email), "Vui lòng nhập email");
            }
            else if (!ModelState.IsValid)
            {
                SetFormViewData("Sửa người dùng", "Sửa");
                return View("Form", model);
            }

            if (!ModelState.IsValid)
            {
                SetFormViewData("Sửa người dùng", "Sửa");
                return View("Form", model);
            }

            var result = await _service.UpdateAsync(model);

            if (!result.Success)
            {
                model.ErrorMessage = result.Message;
                SetFormViewData("Sửa người dùng", "Sửa");
                return View("Form", model);
            }

            TempData["SuccessMessage"] = "Cập nhật người dùng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.USER_DELETE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var (result, blockedMessage) = await _service.DeleteAsync(id);
            if (blockedMessage != null)
            {
                TempData["ErrorMessage"] = blockedMessage;
                return RedirectToAction(nameof(Index));
            }

            TempData[result!.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa người dùng thành công." : (result.Message ?? "Không thể xóa người dùng.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.USER_UPDATE)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, int status, int pageIndex = 1, int? userType = null)
        {
            var (result, blockedMessage) = await _service.UpdateStatusAsync(id, status);
            if (blockedMessage != null)
            {
                TempData["ErrorMessage"] = blockedMessage;
                return RedirectToAction(nameof(Index), new { pageIndex, userType });
            }

            TempData[result!.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Cập nhật trạng thái thành công." : (result.Message ?? "Không thể cập nhật trạng thái.");
            return RedirectToAction(nameof(Index), new { pageIndex, userType });
        }

        public async Task<IActionResult> ResetPassword(long id)
        {
            var (model, errorMessage) = await _service.GetResetPasswordFormAsync(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            SetFormViewData("Reset mật khẩu", "Reset mật khẩu");
            return View(model);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.USER_RESET_PASSWORD)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetFormViewData("Reset mật khẩu", "Reset mật khẩu");
                return View(model);
            }

            var (result, blockedMessage) = await _service.ResetPasswordAsync(model.Id, model.NewPassword);
            if (blockedMessage != null)
            {
                TempData["ErrorMessage"] = blockedMessage;
                return RedirectToAction(nameof(Index));
            }

            if (!result!.Success)
            {
                model.ErrorMessage = result.Message;
                SetFormViewData("Reset mật khẩu", "Reset mật khẩu");
                return View(model);
            }

            TempData["SuccessMessage"] = "Đã reset mật khẩu thành công.";
            return RedirectToAction(nameof(Index));
        }

        private void SetFormViewData(string title, string breadcrumb)
        {
            ViewData["Menu"] = "AppUser";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }
    }
}
