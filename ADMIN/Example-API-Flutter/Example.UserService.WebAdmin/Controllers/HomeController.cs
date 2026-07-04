using Example.UserService.WebAdmin.Authorization;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Const;
using Example.UserService.WebAdmin.Models;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [AuthCustom]
    public class HomeController : Controller
    {
        private readonly IHomeService _service;

        public HomeController(IHomeService service) => _service = service;

        public async Task<IActionResult> Index()
        {
            ViewData["Menu"] = "Dashboard";
            ViewData["PageTitle"] = "Bảng điều khiển";
            ViewData["Title"] = "Bảng điều khiển";

            var vm = await _service.BuildDashboardAsync(
                HttpContext.Session.GetString(SessionKeys.FullName),
                HttpContext.Session.GetString(SessionKeys.Username));

            return View(vm);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
