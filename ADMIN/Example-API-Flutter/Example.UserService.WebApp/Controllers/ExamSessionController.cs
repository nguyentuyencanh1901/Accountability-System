using Example.API.Client.Configuration;
using Example.Common.Enums;
using Example.UserService.WebApp.Authorization;
using AuthCustom = Example.UserService.WebApp.Authorization.AuthorizeAttribute;
using Example.UserService.WebApp.Helpers;
using Example.UserService.WebApp.Models.ExamSession;
using Example.UserService.WebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Example.UserService.WebApp.Controllers
{
    [Authorize]
    public class ExamSessionController : Controller
    {
        private readonly IExamSessionService _service;
        private readonly string _apiBaseUrl;

        public ExamSessionController(IExamSessionService service, IOptions<ApiClientOptions> apiOptions)
        {
            _service = service;
            _apiBaseUrl = apiOptions.Value.BaseUrl ?? string.Empty;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            ViewData["Title"] = "Bài thi đang làm";
            ViewData["ActiveMenu"] = "InProgress";

            var userId = SessionHelper.GetUserId(HttpContext);
            var (items, total, error) = await _service.GetInProgressListAsync(userId, pageIndex);

            return View(new ExamSessionIndexViewModel
            {
                Items = items,
                TotalItems = total,
                PageIndex = pageIndex,
                ErrorMessage = error
            });
        }

        public async Task<IActionResult> History(int pageIndex = 1)
        {
            ViewData["Title"] = "Lịch sử thi";
            ViewData["ActiveMenu"] = "History";

            var userId = SessionHelper.GetUserId(HttpContext);
            var (items, total, error) = await _service.GetHistoryListAsync(userId, pageIndex);

            return View(new ExamSessionHistoryViewModel
            {
                Items = items,
                TotalItems = total,
                PageIndex = pageIndex,
                ErrorMessage = error
            });
        }

        public async Task<IActionResult> Details(long id)
        {
            ViewData["Title"] = "Kết quả bài thi";

            var userId = SessionHelper.GetUserId(HttpContext);
            var (session, error) = await _service.GetOwnedSessionAsync(id, userId);
            if (session == null)
            {
                TempData["ErrorMessage"] = error;
                return RedirectToAction(nameof(Index));
            }

            ViewData["ActiveMenu"] = session.Status == (int)ExamSessionStatusEnum.InProgress ? "InProgress" : "History";

            return View(new ExamSessionDetailsViewModel { Session = session, ApiBaseUrl = _apiBaseUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(long examSetId, int examType, long examPeriodAssignmentId = 0)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            var result = await _service.StartExamAsync(examSetId, examType, examPeriodAssignmentId, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                if (result.ExamPeriodAssignmentId > 0)
                    return RedirectToAction("Details", "ExamSet", new { id = result.ExamPeriodAssignmentId });
                return RedirectToAction("Details", "ExamSet", new { id = result.RedirectExamSetId });
            }

            if (!string.IsNullOrWhiteSpace(result.ResumeMessage))
                TempData["SuccessMessage"] = result.ResumeMessage;

            return RedirectToAction(nameof(Take), new { id = result.SessionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProgress(SubmitExamFormModel model)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            var (success, message) = await _service.SaveProgressAsync(model, userId);
            return Json(new { success, message });
        }

        public async Task<IActionResult> Take(long id)
        {
            ViewData["Title"] = "Làm bài thi";
            ViewData["ActiveMenu"] = "InProgress";

            var userId = SessionHelper.GetUserId(HttpContext);
            var (viewModel, successMessage, error, redirectToDetails, sessionId) = await _service.GetTakeExamAsync(id, userId);

            if (viewModel == null)
            {
                if (!string.IsNullOrWhiteSpace(error))
                    TempData["ErrorMessage"] = error;
                if (!string.IsNullOrWhiteSpace(successMessage))
                    TempData["SuccessMessage"] = successMessage;
                return redirectToDetails
                    ? RedirectToAction(nameof(Details), new { id = sessionId })
                    : RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelDueToViolation(SubmitExamFormModel model)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            var (success, errorMessage, sessionId) = await _service.CancelDueToViolationAsync(model, userId);

            if (!success)
                return Json(new { success = false, message = errorMessage });

            TempData["ErrorMessage"] = model.ViolationCount > 0
                ? $"Bài thi đã bị hủy do vi phạm quy chế thi ({model.ViolationCount} lần vi phạm)."
                : "Bài thi đã bị hủy do vi phạm quy chế thi.";
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Details), new { id = sessionId }) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitExamFormModel model)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            var (success, errorMessage, sessionId, redirectToDetails, showSuccessMessage) = await _service.SubmitExamAsync(model, userId);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Take), new { id = model.ExamSessionId });
            }

            if (showSuccessMessage)
                TempData["SuccessMessage"] = "Nộp bài thành công.";

            return RedirectToAction(nameof(Details), new { id = sessionId });
        }
    }
}
