using Example.Common.Enums;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Helpers;
using Example.UserService.WebAdmin.Models.ExamPeriodAssignment;
using Example.UserService.WebAdmin.Models.Shared;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho phân công kỳ thi: phân trang, form CRUD, validate lựa chọn, map ViewModel ↔ API model.
    /// Gọi repository (ủy quyền HTTP API) và các repository liên quan để nạp dropdown.
    /// </summary>
    public class ExamPeriodAssignmentService : IExamPeriodAssignmentService
    {
        // Repository ủy quyền gọi HTTP API phân công kỳ thi
        private readonly IExamPeriodAssignmentRepository _repository;
        // Repository kỳ thi — dùng lấy tên kỳ và nạp dropdown
        private readonly IExamPeriodRepository _examPeriodRepository;
        // Repository người dùng — dùng nạp dropdown thí sinh
        private readonly IAppUserRepository _appUserRepository;
        // Repository bộ đề — dùng nạp dropdown bộ đề
        private readonly IExamSetRepository _examSetRepository;

        /// <summary>Khởi tạo service với các repository liên quan.</summary>
        public ExamPeriodAssignmentService(
            IExamPeriodAssignmentRepository repository,
            IExamPeriodRepository examPeriodRepository,
            IAppUserRepository appUserRepository,
            IExamSetRepository examSetRepository)
        {
            _repository = repository;
            _examPeriodRepository = examPeriodRepository;
            _appUserRepository = appUserRepository;
            _examSetRepository = examSetRepository;
        }

        /// <summary>Danh sách phân trang; lọc theo kỳ thi; dùng ApiListHelper.ParseList để tách items/total/error.</summary>
        public async Task<(List<ExamPeriodAssignmentModel> Items, long TotalItems, string? ExamPeriodName, string? ErrorMessage)> GetListPagingAsync(long? examPeriodId, int pageIndex)
        {
            string? examPeriodName = null;
            // Lấy tên kỳ thi hiển thị tiêu đề trang khi có examPeriodId hợp lệ
            if (examPeriodId > 0)
            {
                var periodResult = await _examPeriodRepository.GetByIdAsync(examPeriodId.Value);
                examPeriodName = periodResult.Data?.Name;
            }

            // Xây search model: PageSize cố định 10; gắn ExamPeriodId nếu đang lọc theo kỳ
            var search = new ExamPeriodAssignmentSearchModel { PageIndex = pageIndex, PageSize = 10 };
            if (examPeriodId > 0) search.ExamPeriodId = examPeriodId;

            var result = await _repository.GetListAsync(search);
            // ApiListHelper chuẩn hóa ResponseData → (danh sách, tổng bản ghi, thông báo lỗi)
            var (items, total, error) = ApiListHelper.ParseList(result);
            return (items, total, examPeriodName, error);
        }

        /// <summary>Tạo form thêm mới; gán sẵn ExamPeriodId từ query string nếu có.</summary>
        public async Task<ExamPeriodAssignmentFormViewModel> BuildCreateFormAsync(long? examPeriodId)
        {
            var model = new ExamPeriodAssignmentFormViewModel();
            if (examPeriodId > 0) model.ExamPeriodId = examPeriodId.Value;
            await PopulateFormOptionsAsync(model);
            return model;
        }

        /// <summary>Lấy form sửa: map API model → ViewModel bằng Mapster, nạp dropdown.</summary>
        public async Task<(ExamPeriodAssignmentFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy phân công.");

            var model = result.Data.Adapt<ExamPeriodAssignmentFormViewModel>();
            await PopulateFormOptionsAsync(model);
            return (model, null);
        }

        /// <summary>Validate tạo mới: bắt buộc chọn ít nhất một bộ đề (ExamSetIds).</summary>
        public IEnumerable<(string Key, string Message)> ValidateCreate(ExamPeriodAssignmentFormViewModel model)
        {
            if (!model.ExamSetIds.Any())
                yield return (nameof(model.ExamSetIds), "Vui lòng chọn ít nhất một bộ đề");
        }

        /// <summary>Validate sửa: bắt buộc chọn một bộ đề đơn (ExamSetId).</summary>
        public IEnumerable<(string Key, string Message)> ValidateEdit(ExamPeriodAssignmentFormViewModel model)
        {
            if (model.ExamSetId <= 0)
                yield return (nameof(model.ExamSetId), "Vui lòng chọn bộ đề");
        }

        /// <summary>Map ViewModel → BulkSaveModel và gọi API thêm hàng loạt.</summary>
        public Task<ResponseData<object>> InsertBulkAsync(ExamPeriodAssignmentFormViewModel model)
            => _repository.AddBulkAsync(new ExamPeriodAssignmentBulkSaveModel
            {
                ExamPeriodId = model.ExamPeriodId,
                UserId = model.UserId,
                ExamSetIds = model.ExamSetIds,
                ExamType = model.ExamType,
                Status = model.Status
            });

        /// <summary>Map ViewModel → SaveModel (Mapster) và gọi API cập nhật.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamPeriodAssignmentFormViewModel model)
            => _repository.UpdateAsync(model.Adapt<ExamPeriodAssignmentSaveModel>());

        /// <summary>Cập nhật nhanh trạng thái phân công trên danh sách (Index).</summary>
        public async Task<ResponseData<object>> UpdateStatusAsync(long id, int status)
        {
            var current = await _repository.GetByIdAsync(id);
            if (!current.Success || current.Data == null)
                return new ResponseData<object>(current.Message ?? "Không tìm thấy phân công.");

            var saveModel = current.Data.Adapt<ExamPeriodAssignmentSaveModel>();
            saveModel.Status = status;
            return await _repository.UpdateAsync(saveModel);
        }

        /// <summary>Ủy quyền xóa qua repository.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id)
            => _repository.DeleteAsync(id);

        /// <summary>Nạp dropdown: kỳ thi, thí sinh (Active), bộ đề (Active), loại thi và trạng thái phân công.</summary>
        public async Task PopulateFormOptionsAsync(ExamPeriodAssignmentFormViewModel model)
        {
            var periodResult = await _examPeriodRepository.GetListAsync(new ExamPeriodSearchModel { PageIndex = 1, PageSize = 500 });
            model.ExamPeriodOptions = periodResult.Success && periodResult.Data != null
                ? periodResult.Data.Select(p => new SelectListItem(p.Name, p.Id.ToString(), p.Id == model.ExamPeriodId)).ToList()
                : new List<SelectListItem>();

            // Lọc thí sinh: UserType = TestTaker, Status = Active
            var userResult = await _appUserRepository.GetListAsync(new AppUserSearchModel
            {
                PageIndex = 1,
                PageSize = 500,
                UserType = (int)UserTypeEnum.TestTaker,
                Status = (int)StatusEnum.Active
            });
            model.UserOptions = userResult.Success && userResult.Data != null
                ? userResult.Data.Select(u => new SelectListItem($"{u.FullName} ({u.Username})", u.Id.ToString(), u.Id == model.UserId)).ToList()
                : new List<SelectListItem>();

            // Chỉ bộ đề đang hoạt động; chọn nhiều khi tạo, chọn một khi sửa
            var examSetResult = await _examSetRepository.GetListAsync(new ExamSetSearchModel { PageIndex = 1, PageSize = 500, Status = (int)StatusEnum.Active });
            var examSets = examSetResult.Success && examSetResult.Data != null ? examSetResult.Data : new List<ExamSetModel>();
            var examSetType = ExamSetTypeHelper.MapExamTypeToExamSetType(model.ExamType);

            model.ExamSetPickOptions = examSets.Select(e => new ExamSetPickOptionViewModel
            {
                Id = e.Id,
                Name = e.Name,
                Type = e.Type,
                Selected = model.IsEdit ? e.Id == model.ExamSetId : model.ExamSetIds.Contains(e.Id)
            }).ToList();

            model.ExamSetOptions = examSets
                .Where(e => e.Type == examSetType)
                .Select(e => new SelectListItem(
                    e.Name,
                    e.Id.ToString(),
                    model.IsEdit ? e.Id == model.ExamSetId : model.ExamSetIds.Contains(e.Id)))
                .ToList();

            model.ExamTypeOptions = SelectListHelper.ExamTypes(model.ExamType);
            model.StatusOptions = SelectListHelper.ExamPeriodAssignmentStatuses(model.Status);
        }

        /// <summary>Hiển thị tên loại thi trên UI (enum → tiếng Việt).</summary>
        public string GetExamTypeName(int examType) => examType switch
        {
            (int)ExamTypeEnum.Trial => "Thi thử",
            (int)ExamTypeEnum.Real => "Thi thật",
            _ => examType.ToString()
        };

        /// <summary>Hiển thị tên trạng thái phân công trên UI (enum → tiếng Việt).</summary>
        public string GetStatusName(int status) => status switch
        {
            (int)ExamPeriodAssignmentStatusEnum.Assigned => "Đã phân công",
            (int)ExamPeriodAssignmentStatusEnum.InProgress => "Đang thi",
            (int)ExamPeriodAssignmentStatusEnum.Completed => "Hoàn thành",
            (int)ExamPeriodAssignmentStatusEnum.Absent => "Vắng thi",
            (int)ExamPeriodAssignmentStatusEnum.Cancelled => "Bị hủy bỏ",
            _ => status.ToString()
        };
    }
}
