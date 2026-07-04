using Example.UserService.WebApp.Authorization;
using Example.UserService.WebApp.Const;
using Example.UserService.WebApp.Helpers;
using Example.UserService.WebApp.Models.Profile;
using Example.UserService.WebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _service;

        public ProfileController(IProfileService service) => _service = service;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Thông tin cá nhân";
            ViewData["ActiveMenu"] = "Profile";

            var userId = SessionHelper.GetUserId(HttpContext);
            return View(await _service.GetProfileAsync(userId));
        }

        public async Task<IActionResult> Edit()
        {
            ViewData["Title"] = "Sửa thông tin";
            ViewData["ActiveMenu"] = "Profile";

            var userId = SessionHelper.GetUserId(HttpContext);
            var model = await _service.BuildEditFormAsync(userId);
            if (!string.IsNullOrWhiteSpace(model.ErrorMessage) && string.IsNullOrWhiteSpace(model.Username))
            {
                TempData["ErrorMessage"] = model.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            ViewData["Title"] = "Sửa thông tin";
            ViewData["ActiveMenu"] = "Profile";

            if (!ModelState.IsValid)
                return View(model);

            var userId = SessionHelper.GetUserId(HttpContext);
            var (success, errorMessage, fullName, email) = await _service.UpdateProfileAsync(userId, model);
            if (!success)
            {
                model.ErrorMessage = errorMessage;
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(fullName))
                HttpContext.Session.SetString(SessionKeys.FullName, fullName);
            if (!string.IsNullOrWhiteSpace(email))
                HttpContext.Session.SetString(SessionKeys.Email, email);

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangePassword()
        {
            ViewData["Title"] = "Đổi mật khẩu";
            ViewData["ActiveMenu"] = "Profile";
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ViewData["Title"] = "Đổi mật khẩu";
            ViewData["ActiveMenu"] = "Profile";

            if (!ModelState.IsValid)
                return View(model);

            var userId = SessionHelper.GetUserId(HttpContext);
            var (success, errorMessage) = await _service.ChangePasswordAsync(userId, model);
            if (!success)
            {
                model.ErrorMessage = errorMessage;
                return View(model);
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
