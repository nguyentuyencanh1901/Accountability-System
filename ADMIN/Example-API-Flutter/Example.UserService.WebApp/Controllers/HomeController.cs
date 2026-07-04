using Example.UserService.WebApp.Authorization;
using AuthCustom = Example.UserService.WebApp.Authorization.AuthorizeAttribute;
using Example.UserService.WebApp.Const;
using Example.UserService.WebApp.Helpers;
using Example.UserService.WebApp.Models;
using Example.UserService.WebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebApp.Controllers
{
    [AuthCustom]
    public class HomeController : Controller
    {
        private readonly IHomeService _service;

        public HomeController(IHomeService service) => _service = service;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Trang chủ";
            ViewData["ActiveMenu"] = "Home";

            var userId = SessionHelper.GetUserId(HttpContext);
            var vm = await _service.BuildDashboardAsync(
                userId,
                HttpContext.Session.GetString(SessionKeys.FullName),
                HttpContext.Session.GetString(SessionKeys.Username));

            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
