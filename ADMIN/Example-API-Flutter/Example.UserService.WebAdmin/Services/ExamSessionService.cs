using Example.Common.Enums;
using Example.Common.Models;
using Example.Common.Utilities;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Helpers;
using Example.UserService.WebAdmin.Models.ExamSession;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho phiên/bài thi: phân trang (ApiListHelper), form CRUD với giá trị mặc định,
    /// nạp dropdown bộ đề/thí sinh, map ViewModel ↔ SaveModel, hiển thị enum tiếng Việt.
    /// </summary>
    public class ExamSessionService : IExamSessionService
    {
        // Repository ủy quyền gọi HTTP API phiên/bài thi
        private readonly IExamSessionRepository _repository;
        // Repository bộ đề — dùng nạp dropdown bộ đề
        private readonly IExamSetRepository _examSetRepository;
        // Repository người dùng — dùng nạp dropdown thí sinh
        private readonly IAppUserRepository _appUserRepository;

        /// <summary>Khởi tạo service với các repository liên quan.</summary>
        public ExamSessionService(
            IExamSessionRepository repository,
            IExamSetRepository examSetRepository,
            IAppUserRepository appUserRepository)
        {
            _repository = repository;
            _examSetRepository = examSetRepository;
            _appUserRepository = appUserRepository;
        }

        /// <summary>Danh sách phân trang PageSize=10; ApiListHelper.ParseList tách items/total/error.</summary>
        public async Task<(List<ExamSessionModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex)
        {
            var result = await _repository.GetListAsync(new ExamSessionSearchModel { PageIndex = pageIndex, PageSize = 10 });
            return ApiListHelper.ParseList(result);
        }

        /// <summary>Tạo form thêm mới với loại thi Thi thử và trạng thái Đang thi mặc định.</summary>
        public async Task<ExamSessionFormViewModel> BuildCreateFormAsync()
        {
            var model = new ExamSessionFormViewModel
            {
                ExamType = (int)ExamTypeEnum.Trial,
                Status = (int)ExamSessionStatusEnum.InProgress,
                StartedAt = VietnamTimeHelper.ToVietnamTime(DateTimeOffset.UtcNow)
            };
            await PopulateFormOptionsAsync(model);
            return model;
        }

        /// <summary>Lấy form sửa: map API → ViewModel (Mapster), nạp dropdown.</summary>
        public async Task<(ExamSessionFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy bài thi.");

            var model = result.Data.Adapt<ExamSessionFormViewModel>();
            model.StartedAt = VietnamTimeHelper.ToVietnamTime(model.StartedAt);
            if (model.FinishedAt.HasValue)
                model.FinishedAt = VietnamTimeHelper.ToVietnamTime(model.FinishedAt.Value);
            await PopulateFormOptionsAsync(model);
            return (model, null);
        }

        /// <summary>Trang chi tiết: bọc kết quả GetById vào DetailViewModel (không map sang form).</summary>
        public async Task<(ExamSessionDetailViewModel Model, string? ErrorMessage)> GetDetailsAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (new ExamSessionDetailViewModel { ErrorMessage = result.Message ?? "Không tìm thấy bài thi." }, null);

            return (new ExamSessionDetailViewModel { Item = result.Data }, null);
        }

        /// <summary>Map ViewModel → SaveModel và gọi API thêm.</summary>
        public Task<ResponseData<object>> InsertAsync(ExamSessionFormViewModel model)
            => _repository.AddAsync(ToSaveModel(model));

        /// <summary>Map ViewModel → SaveModel và gọi API cập nhật.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamSessionFormViewModel model)
            => _repository.UpdateAsync(ToSaveModel(model));

        /// <summary>Cập nhật nhanh trạng thái bài thi trên danh sách (Index).</summary>
        public async Task<ResponseData<object>> UpdateStatusAsync(long id, int status)
        {
            var current = await _repository.GetByIdAsync(id);
            if (!current.Success || current.Data == null)
                return new ResponseData<object>(current.Message ?? "Không tìm thấy bài thi.");

            var saveModel = current.Data.Adapt<ExamSessionSaveModel>();
            saveModel.Status = status;
            return await _repository.UpdateAsync(saveModel);
        }

        /// <summary>Ủy quyền xóa qua repository.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id)
            => _repository.DeleteAsync(id);

        /// <summary>Nạp dropdown loại thi, trạng thái, bộ đề và thí sinh (UserType = TestTaker).</summary>
        public async Task PopulateFormOptionsAsync(ExamSessionFormViewModel model)
        {
            model.ExamTypeOptions = SelectListHelper.ExamTypes(model.ExamType);
            model.StatusOptions = SelectListHelper.ExamSessionStatuses(model.Status);

            var examSetResult = await _examSetRepository.GetListAsync(new ExamSetSearchModel { PageIndex = 1, PageSize = 500 });
            model.ExamSetOptions = examSetResult.Success && examSetResult.Data != null
                ? examSetResult.Data.Select(e => new SelectListItem(
                    e.Name, e.Id.ToString(), e.Id == model.ExamSetId)).ToList()
                : new List<SelectListItem>();

            var userResult = await _appUserRepository.GetListAsync(new AppUserSearchModel
            {
                PageIndex = 1,
                PageSize = 500,
                UserType = (int)UserTypeEnum.TestTaker
            });
            model.UserOptions = userResult.Success && userResult.Data != null
                ? userResult.Data.Select(u => new SelectListItem(
                    $"{u.FullName} ({u.Username})", u.Id.ToString(), u.Id == model.UserId)).ToList()
                : new List<SelectListItem>();
        }

        /// <summary>Hiển thị tên loại thi trên UI.</summary>
        public string GetExamTypeName(int examType) => examType switch
        {
            (int)ExamTypeEnum.Trial => "Thi thử",
            (int)ExamTypeEnum.Real => "Thi thật",
            _ => examType.ToString()
        };

        private static ExamSessionSaveModel ToSaveModel(ExamSessionFormViewModel model)
        {
            var save = model.Adapt<ExamSessionSaveModel>();
            save.StartedAt = VietnamTimeHelper.NormalizeFromForm(model.StartedAt);
            save.FinishedAt = model.FinishedAt.HasValue
                ? VietnamTimeHelper.NormalizeFromForm(model.FinishedAt.Value)
                : null;
            return save;
        }

        /// <summary>Hiển thị tên trạng thái bài thi trên UI.</summary>
        public string GetStatusName(int status) => status switch
        {
            (int)ExamSessionStatusEnum.InProgress => "Đang thi",
            (int)ExamSessionStatusEnum.Completed => "Hoàn thành",
            (int)ExamSessionStatusEnum.Expired => "Hết hạn",
            (int)ExamSessionStatusEnum.Cancelled => "Bị hủy bỏ",
            _ => status.ToString()
        };
    }
}
