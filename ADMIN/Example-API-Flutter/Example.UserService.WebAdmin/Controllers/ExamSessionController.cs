using Example.UserService.WebAdmin.Authorization;
using Example.Common.Enums;
using AuthCustom = Example.UserService.WebAdmin.Authorization.AuthorizeAttribute;
using Example.UserService.WebAdmin.Models.ExamSession;
using Example.UserService.WebAdmin.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Example.UserService.WebAdmin.Controllers
{
    [Authorize]
    public class ExamSessionController : Controller
    {
        private readonly IExamSessionService _service;

        public ExamSessionController(IExamSessionService service) => _service = service;

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            ViewData["Menu"] = "ExamSession";
            ViewData["PageTitle"] = "Bài thi";
            ViewData["Breadcrumb"] = "Bài thi";
            ViewData["Title"] = "Bài thi";

            var (items, total, error) = await _service.GetListPagingAsync(pageIndex);
            return View(new ExamSessionIndexViewModel { Items = items, TotalItems = total, PageIndex = pageIndex, ErrorMessage = error });
        }

        public async Task<IActionResult> Create()
        {
            SetFormViewData("Thêm bài thi", "Thêm mới");
            return View("Form", await _service.BuildCreateFormAsync());
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_CREATE)]
        public async Task<IActionResult> Create(ExamSessionFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Thêm bài thi", "Thêm mới"); return View("Form", model); }

            var result = await _service.InsertAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Thêm bài thi", "Thêm mới"); return View("Form", model); }

            TempData["SuccessMessage"] = "Thêm bài thi thành công.";
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

            SetFormViewData("Sửa bài thi", "Sửa");
            return View("Form", formModel);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_UPDATE)]
        public async Task<IActionResult> Edit(ExamSessionFormViewModel model)
        {
            await _service.PopulateFormOptionsAsync(model);
            if (!ModelState.IsValid) { SetFormViewData("Sửa bài thi", "Sửa"); return View("Form", model); }

            var result = await _service.UpdateAsync(model);
            if (!result.Success) { model.ErrorMessage = result.Message; SetFormViewData("Sửa bài thi", "Sửa"); return View("Form", model); }

            TempData["SuccessMessage"] = "Cập nhật bài thi thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(long id)
        {
            ViewData["Menu"] = "ExamSession";
            ViewData["PageTitle"] = "Chi tiết bài thi";
            ViewData["Breadcrumb"] = "Chi tiết";
            ViewData["Title"] = "Chi tiết bài thi";

            var (model, errorMessage) = await _service.GetDetailsAsync(id);
            return View(model);
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_DELETE)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Xóa bài thi thành công." : result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AuthCustom(PermissionCodeEnum.EXAMSESSION_UPDATE)]
        public async Task<IActionResult> UpdateStatus(long id, int status, int pageIndex = 1)
        {
            var result = await _service.UpdateStatusAsync(id, status);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Cập nhật trạng thái thành công." : (result.Message ?? "Không thể cập nhật trạng thái.");
            return RedirectToAction(nameof(Index), new { pageIndex });
        }

        private void SetFormViewData(string title, string breadcrumb)
        {
            ViewData["Menu"] = "ExamSession";
            ViewData["PageTitle"] = title;
            ViewData["Breadcrumb"] = breadcrumb;
            ViewData["Title"] = title;
        }
    }
}
