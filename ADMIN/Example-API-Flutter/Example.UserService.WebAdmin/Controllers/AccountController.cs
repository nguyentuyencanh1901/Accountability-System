using Example.API.Client.Core;
using Example.UserService.WebAdmin.Const;
using Example.UserService.WebAdmin.Models;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticateService _service;
        private readonly IApiTokenProvider _tokenProvider;

        public AccountController(IAuthenticateService service, IApiTokenProvider tokenProvider)
        {
            _service = service;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            var model = new LoginViewModel
            {
                SuccessMessage = TempData["SuccessMessage"] as string
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage, data) = await _service.LoginAsync(model);
            if (!success || data == null)
            {
                model.ErrorMessage = errorMessage;
                return View(model);
            }

            HttpContext.Session.SetString(SessionKeys.Token, data.Token);
            HttpContext.Session.SetString(SessionKeys.UserId, data.User.Id.ToString());
            HttpContext.Session.SetString(SessionKeys.Username, data.User.Username);
            HttpContext.Session.SetString(SessionKeys.FullName, data.User.FullName);
            HttpContext.Session.SetString(SessionKeys.Email, data.User.Email);
            HttpContext.Session.SetInt32(SessionKeys.UserType, data.User.UserType);

            _tokenProvider.Token = data.Token;

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            // Tài khoản quản trị chỉ được Super Admin tạo trong mục Người dùng.
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _service.Logout();
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewData["Menu"] = "Dashboard";
            ViewData["PageTitle"] = "Đổi mật khẩu";
            ViewData["Title"] = "Đổi mật khẩu";
            return View(new ChangePasswordViewModel
            {
                SuccessMessage = TempData["SuccessMessage"] as string
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ViewData["Menu"] = "Dashboard";
            ViewData["PageTitle"] = "Đổi mật khẩu";
            ViewData["Title"] = "Đổi mật khẩu";

            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage) = await _service.ChangePasswordAsync(model);
            if (!success)
            {
                model.ErrorMessage = errorMessage;
                return View(model);
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
