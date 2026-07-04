using Example.Common.Enums;
using Example.Common.Models;
using Example.Common.Utilities;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Helpers;
using Example.UserService.WebAdmin.Models.ExamPeriod;
using Example.UserService.WebAdmin.Models.Shared;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho kỳ thi: CRUD, phân trang, form, validate lựa chọn, chuyển múi giờ VN,
    /// lọc client-side màn giám sát, map ViewModel ↔ SaveModel.
    /// </summary>
    public class ExamPeriodService : IExamPeriodService
    {
        // Repository ủy quyền gọi HTTP API kỳ thi
        private readonly IExamPeriodRepository _repository;
        // Repository người dùng — dùng nạp dropdown thí sinh
        private readonly IAppUserRepository _appUserRepository;
        // Repository bộ đề — dùng nạp dropdown bộ đề theo loại
        private readonly IExamSetRepository _examSetRepository;

        /// <summary>Khởi tạo service với các repository liên quan.</summary>
        public ExamPeriodService(
            IExamPeriodRepository repository,
            IAppUserRepository appUserRepository,
            IExamSetRepository examSetRepository)
        {
            _repository = repository;
            _appUserRepository = appUserRepository;
            _examSetRepository = examSetRepository;
        }

        /// <summary>Danh sách phân trang; ApiListHelper.ParseList tách items/total/error từ ResponseData.</summary>
        public async Task<(List<ExamPeriodModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex)
        {
            var result = await _repository.GetListAsync(new ExamPeriodSearchModel { PageIndex = pageIndex, PageSize = 10 });
            return ApiListHelper.ParseList(result);
        }

        /// <summary>Tạo form thêm mới và nạp toàn bộ dropdown.</summary>
        public async Task<ExamPeriodFormViewModel> BuildCreateFormAsync()
        {
            var nowVn = VietnamTimeHelper.NowVietnam();
            var model = new ExamPeriodFormViewModel
            {
                StartAt = nowVn,
                EndAt = nowVn.AddDays(7)
            };
            await PopulateFormOptionsAsync(model);
            return model;
        }

        /// <summary>Lấy form sửa: map API → ViewModel, chuyển StartAt/EndAt sang giờ VN cho input form.</summary>
        public async Task<(ExamPeriodFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy kỳ thi.");

            var model = result.Data.Adapt<ExamPeriodFormViewModel>();
            // API trả UTC/offset; form hiển thị theo múi giờ Việt Nam (+07)
            model.StartAt = VietnamTimeHelper.ToVietnamTime(model.StartAt);
            model.EndAt = VietnamTimeHelper.ToVietnamTime(model.EndAt);
            model.OriginalStartAt = model.StartAt;
            // Suy ra loại bộ đề từ bộ đề đã gắn — tránh mặc định "Thi thật" khi sửa kỳ thi thử
            await SyncExamSetTypeFilterFromSelectionAsync(model);
            await PopulateFormOptionsAsync(model);
            return (model, null);
        }

        /// <summary>
        /// Khi sửa: lấy Type của bộ đề đã chọn để set ExamSetTypeFilter (dropdown + danh sách checkbox khớp nhau).
        /// </summary>
        private async Task SyncExamSetTypeFilterFromSelectionAsync(ExamPeriodFormViewModel model)
        {
            if (model.ExamSetIds == null || model.ExamSetIds.Count == 0)
                return;

            var firstId = model.ExamSetIds.FirstOrDefault(x => x > 0);
            if (firstId <= 0)
                return;

            var examSetResult = await _examSetRepository.GetByIdAsync(firstId);
            if (examSetResult.Success && examSetResult.Data != null && examSetResult.Data.Type > 0)
                model.ExamSetTypeFilter = examSetResult.Data.Type;
        }

        /// <summary>Validate nghiệp vụ: thời gian, bắt buộc chọn thí sinh và bộ đề.</summary>
        public IEnumerable<(string Key, string Message)> ValidateSelections(ExamPeriodFormViewModel model)
        {
            var startAt = VietnamTimeHelper.NormalizeFromForm(model.StartAt);
            var endAt = VietnamTimeHelper.NormalizeFromForm(model.EndAt);
            var nowVn = VietnamTimeHelper.NowVietnam();

            var startUnchanged = model.OriginalStartAt.HasValue
                && VietnamTimeHelper.NormalizeFromForm(model.OriginalStartAt.Value) == startAt;

            if (!startUnchanged && startAt < nowVn.AddMinutes(-1))
                yield return (nameof(model.StartAt), "Thời gian bắt đầu phải từ hiện tại trở đi.");

            if (endAt <= startAt)
                yield return (nameof(model.EndAt), "Thời gian kết thúc phải sau thời gian bắt đầu.");

            if (model.UserIds == null || !model.UserIds.Any())
                yield return (nameof(model.UserIds), "Vui lòng chọn ít nhất một thí sinh");
            if (model.ExamSetIds == null || !model.ExamSetIds.Any())
                yield return (nameof(model.ExamSetIds), "Vui lòng chọn ít nhất một bộ đề");
        }

        /// <summary>Map ViewModel → SaveModel (có xử lý timezone) rồi gọi API thêm.</summary>
        public Task<ResponseData<object>> InsertAsync(ExamPeriodFormViewModel model)
            => _repository.AddAsync(ToSaveModel(model));

        /// <summary>Map ViewModel → SaveModel (có xử lý timezone) rồi gọi API cập nhật.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamPeriodFormViewModel model)
            => _repository.UpdateAsync(ToSaveModel(model));

        /// <summary>Cập nhật nhanh trạng thái kỳ thi trên danh sách (Index).</summary>
        public async Task<ResponseData<object>> UpdateStatusAsync(long id, int status)
        {
            var current = await _repository.GetByIdAsync(id);
            if (!current.Success || current.Data == null)
                return new ResponseData<object>(current.Message ?? "Không tìm thấy kỳ thi.");

            var saveModel = current.Data.Adapt<ExamPeriodSaveModel>();
            saveModel.Status = status;
            return await _repository.UpdateAsync(saveModel);
        }

        /// <summary>Ủy quyền xóa qua repository.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id)
            => _repository.DeleteAsync(id);

        /// <summary>Nạp dropdown trạng thái, thí sinh Active và bộ đề Active theo loại.</summary>
        public async Task PopulateFormOptionsAsync(ExamPeriodFormViewModel model)
        {
            model.StatusOptions = SelectListHelper.ExamPeriodStatuses(model.Status);
            model.ExamSetTypeOptions = SelectListHelper.ExamSetTypes(model.ExamSetTypeFilter);

            var userResult = await _appUserRepository.GetListAsync(new AppUserSearchModel
            {
                PageIndex = 1,
                PageSize = 500,
                UserType = (int)UserTypeEnum.TestTaker,
                Status = (int)StatusEnum.Active
            });
            model.UserOptions = userResult.Success && userResult.Data != null
                ? userResult.Data.Select(u => new SelectListItem(
                    $"{u.FullName} ({u.Username})",
                    u.Id.ToString(),
                    model.UserIds.Contains(u.Id))).ToList()
                : new List<SelectListItem>();

            var examSetResult = await _examSetRepository.GetListAsync(new ExamSetSearchModel
            {
                PageIndex = 1,
                PageSize = 500,
                Status = (int)StatusEnum.Active,
                Type = model.ExamSetTypeFilter
            });
            model.ExamSetOptions = examSetResult.Success && examSetResult.Data != null
                ? examSetResult.Data.Select(e => new ExamSetPickOptionViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Type = e.Type,
                    Selected = model.ExamSetIds.Contains(e.Id)
                }).ToList()
                : new List<ExamSetPickOptionViewModel>();
        }

        /// <summary>Tải danh sách bộ đề Active theo loại (Trial/Real) để chọn trên form.</summary>
        public async Task<List<ExamSetPickOptionViewModel>> GetExamSetOptionsByTypeAsync(int examSetType, List<long>? selectedIds = null)
        {
            var selected = selectedIds?.Where(x => x > 0).ToHashSet() ?? new HashSet<long>();
            var examSetResult = await _examSetRepository.GetListAsync(new ExamSetSearchModel
            {
                PageIndex = 1,
                PageSize = 500,
                Status = (int)StatusEnum.Active,
                Type = examSetType
            });

            var examSets = examSetResult.Success && examSetResult.Data != null ? examSetResult.Data : new List<ExamSetModel>();
            return examSets.Select(e => new ExamSetPickOptionViewModel
            {
                Id = e.Id,
                Name = e.Name,
                Type = e.Type,
                Selected = selected.Contains(e.Id)
            }).ToList();
        }

        /// <summary>Hiển thị tên trạng thái kỳ thi trên UI.</summary>
        public string GetStatusName(int status) => status switch
        {
            (int)ExamPeriodStatusEnum.Draft => "Nháp",
            (int)ExamPeriodStatusEnum.Published => "Đã công bố",
            (int)ExamPeriodStatusEnum.Closed => "Đã đóng",
            _ => status.ToString()
        };

        /// <summary>
        /// Màn giám sát: gọi API monitoring một lần, lọc phía WebAdmin theo status/examType (không gọi lại API).
        /// </summary>
        public async Task<(ExamPeriodDetailViewModel? Model, string? ErrorMessage)> GetMonitoringAsync(long id, int? status, int? examType)
        {
            var result = await _repository.GetMonitoringAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy kỳ thi.");

            var data = result.Data;
            var assignments = data.Assignments.AsEnumerable();

            // Lọc client-side: chỉ áp dụng khi tham số > 0 (0 hoặc null = tất cả)
            if (status > 0)
                assignments = assignments.Where(x => x.Status == status.Value);
            if (examType > 0)
                assignments = assignments.Where(x => x.ExamType == examType.Value);

            var model = new ExamPeriodDetailViewModel
            {
                Period = data.Period,
                AssignedCount = data.AssignedCount,
                InProgressCount = data.InProgressCount,
                CompletedCount = data.CompletedCount,
                AbsentCount = data.AbsentCount,
                Assignments = assignments.ToList(),
                FilterStatus = status,
                FilterExamType = examType
            };
            return (model, null);
        }

        /// <summary>Định dạng thời lượng làm bài (giây/phút/giờ) cho hiển thị trên UI.</summary>
        public string FormatDuration(DateTimeOffset? startedAt, DateTimeOffset? finishedAt)
        {
            if (!startedAt.HasValue)
                return "-";
            var end = finishedAt ?? DateTimeOffset.Now;
            var span = end - startedAt.Value;
            if (span.TotalMinutes < 1)
                return $"{(int)span.TotalSeconds} giây";
            if (span.TotalHours < 1)
                return $"{(int)span.TotalMinutes} phút";
            return $"{(int)span.TotalHours} giờ {(int)span.Minutes} phút";
        }

        /// <summary>
        /// Map ViewModel → SaveModel: Mapster + chuẩn hóa datetime form sang offset +07;
        /// loại bỏ UserIds/ExamSetIds trùng hoặc &lt;= 0.
        /// </summary>
        private static ExamPeriodSaveModel ToSaveModel(ExamPeriodFormViewModel model)
        {
            var save = model.Adapt<ExamPeriodSaveModel>();
            // Giá trị từ input dd/MM/yyyy HH:mm → DateTimeOffset múi giờ VN trước khi gửi API
            save.StartAt = VietnamTimeHelper.NormalizeFromForm(model.StartAt);
            save.EndAt = VietnamTimeHelper.NormalizeFromForm(model.EndAt);
            // Làm sạch danh sách id thí sinh và bộ đề
            save.UserIds = model.UserIds?.Where(x => x > 0).Distinct().ToList() ?? new List<long>();
            save.ExamSetIds = model.ExamSetIds?.Where(x => x > 0).Distinct().ToList() ?? new List<long>();
            return save;
        }
    }
}
