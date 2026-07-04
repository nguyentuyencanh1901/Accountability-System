using Example.API.Client.Core;
using Example.API.Client.Models;
using Example.UserService.WebApp.Const;
using Example.UserService.WebApp.Models;
using Example.UserService.WebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebApp.Controllers
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

            return View(new LoginViewModel
            {
                SuccessMessage = TempData["SuccessMessage"] as string
            });
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

            SaveSession(data);
            _tokenProvider.Token = data.Token;
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage) = await _service.RegisterAsync(model);
            if (!success)
            {
                model.ErrorMessage = errorMessage;
                return View(model);
            }

            TempData["SuccessMessage"] = "Đăng ký tài khoản thí sinh thành công. Vui lòng đăng nhập.";
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
        public IActionResult AccessDenied() => View();

        private void SaveSession(LoginResponse data)
        {
            // Lưu JWT + thông tin user; SessionTokenMiddleware đẩy token sang mọi request API.
            HttpContext.Session.SetString(SessionKeys.Token, data.Token);
            HttpContext.Session.SetString(SessionKeys.UserId, data.User.Id.ToString());
            HttpContext.Session.SetString(SessionKeys.Username, data.User.Username);
            HttpContext.Session.SetString(SessionKeys.FullName, data.User.FullName);
            HttpContext.Session.SetString(SessionKeys.Email, data.User.Email);
            HttpContext.Session.SetInt32(SessionKeys.UserType, data.User.UserType);
        }
    }
}
