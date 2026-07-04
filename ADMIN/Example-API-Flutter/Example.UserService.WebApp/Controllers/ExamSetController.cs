using Example.UserService.WebApp.Authorization;
using AuthCustom = Example.UserService.WebApp.Authorization.AuthorizeAttribute;
using Example.UserService.WebApp.Helpers;
using Example.UserService.WebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebApp.Controllers
{
    [AuthCustom]
    public class ExamSetController : Controller
    {
        private readonly IExamSetService _service;

        public ExamSetController(IExamSetService service) => _service = service;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Kỳ thi của tôi";
            ViewData["ActiveMenu"] = "ExamSet";

            return View(await _service.GetIndexAsync());
        }

        public async Task<IActionResult> Details(long id)
        {
            ViewData["Title"] = "Chi tiết kỳ thi";
            ViewData["ActiveMenu"] = "ExamSet";

            var userId = SessionHelper.GetUserId(HttpContext);
            var (model, errorMessage) = await _service.GetDetailsAsync(id, userId);
            if (model == null)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}
