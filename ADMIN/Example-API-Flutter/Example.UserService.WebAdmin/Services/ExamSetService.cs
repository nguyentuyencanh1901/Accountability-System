using Example.Common.Enums;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Helpers;
using Example.UserService.WebAdmin.Models.ExamSet;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;
using Mapster;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho bộ đề: phân trang (ApiListHelper), form CRUD, nạp dropdown lĩnh vực,
    /// danh sách thí sinh đã làm bộ đề, map ViewModel ↔ SaveModel, hiển thị enum tiếng Việt.
    /// </summary>
    public class ExamSetService : IExamSetService
    {
        // Repository ủy quyền gọi HTTP API bộ đề
        private readonly IExamSetRepository _repository;
        // Repository lĩnh vực — dùng nạp checkbox lĩnh vực trên form
        private readonly IFieldRepository _fieldRepository;

        /// <summary>Khởi tạo service với repository bộ đề và lĩnh vực.</summary>
        public ExamSetService(IExamSetRepository repository, IFieldRepository fieldRepository)
        {
            _repository = repository;
            _fieldRepository = fieldRepository;
        }

        /// <summary>Danh sách phân trang PageSize=10; ApiListHelper.ParseList tách items/total/error.</summary>
        public async Task<(List<ExamSetModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex)
        {
            var result = await _repository.GetListAsync(new ExamSetSearchModel { PageIndex = pageIndex, PageSize = 10 });
            return ApiListHelper.ParseList(result);
        }

        /// <summary>Tạo form thêm mới với số câu mặc định và nạp danh sách lĩnh vực.</summary>
        public async Task<ExamSetFormViewModel> BuildCreateFormAsync()
        {
            var model = BuildFormViewModel(new ExamSetFormViewModel
            {
                MediumCount = 10,
                QuestionCount = 10
            });
            await PopulateFieldOptionsAsync(model);
            return model;
        }

        /// <summary>Lấy form sửa: map API → ViewModel (Mapster), nạp dropdown lĩnh vực.</summary>
        public async Task<(ExamSetFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy bộ đề.");

            var model = result.Data.Adapt<ExamSetFormViewModel>();
            model = BuildFormViewModel(model);
            await PopulateFieldOptionsAsync(model);
            return (model, null);
        }

        /// <summary>Map ViewModel → SaveModel và gọi API thêm.</summary>
        public Task<ResponseData<object>> InsertAsync(ExamSetFormViewModel model)
            => _repository.AddAsync(ToSaveModel(model));

        /// <summary>Map ViewModel → SaveModel và gọi API cập nhật.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamSetFormViewModel model)
            => _repository.UpdateAsync(ToSaveModel(model));

        /// <summary>
        /// Cập nhật nhanh trạng thái bộ đề trên danh sách (Index) mà không cần vào form.
        /// </summary>
        public async Task<ResponseData<object>> UpdateStatusAsync(long id, int status)
        {
            var current = await _repository.GetByIdAsync(id);
            if (!current.Success || current.Data == null)
                return new ResponseData<object>(current.Message ?? "Không tìm thấy bộ đề.");

            var saveModel = current.Data.Adapt<ExamSetSaveModel>();
            saveModel.Status = status;
            return await _repository.UpdateAsync(saveModel);
        }

        /// <summary>Ủy quyền xóa qua repository.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id)
            => _repository.DeleteAsync(id);

        /// <summary>Nạp dropdown loại bộ đề và trạng thái cho form.</summary>
        public void PopulateFormOptions(ExamSetFormViewModel model)
        {
            model.TypeOptions = SelectListHelper.ExamSetTypes(model.Type);
            model.StatusOptions = SelectListHelper.Statuses(model.Status);
        }

        /// <summary>Dựng ViewModel form: nạp dropdown loại/trạng thái rồi trả về model.</summary>
        public ExamSetFormViewModel BuildFormViewModel(ExamSetFormViewModel model)
        {
            PopulateFormOptions(model);
            return model;
        }

        /// <summary>Nạp danh sách lĩnh vực Active dạng checkbox (đánh dấu đã chọn theo FieldIds).</summary>
        public async Task PopulateFieldOptionsAsync(ExamSetFormViewModel model)
        {
            var fieldResult = await _fieldRepository.GetListAsync(new FieldSearchModel { PageIndex = 1, PageSize = 500, Status = 1 });
            var fields = fieldResult.Success && fieldResult.Data != null ? fieldResult.Data : new List<FieldModel>();
            var selected = model.FieldIds.ToHashSet();
            model.FieldOptions = fields
                .Select(f => new ExamSetFieldOptionViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    Selected = selected.Contains(f.Id)
                })
                .ToList();
        }

        /// <summary>
        /// Map ViewModel → SaveModel: Mapster + loại bỏ FieldIds trùng hoặc &lt;= 0.
        /// </summary>
        private static ExamSetSaveModel ToSaveModel(ExamSetFormViewModel model)
        {
            var saveModel = model.Adapt<ExamSetSaveModel>();
            saveModel.FieldIds = model.FieldIds?.Where(x => x > 0).Distinct().ToList() ?? new List<long>();
            return saveModel;
        }

        /// <summary>
        /// Lấy thông tin bộ đề và danh sách thí sinh đã làm bài (phân trang PageSize=15).
        /// </summary>
        public async Task<(ExamSetModel? ExamSet, List<ExamSetTakerModel> Items, long TotalItems, string? ErrorMessage)> GetExamTakersAsync(long examSetId, int pageIndex)
        {
            // Kiểm tra bộ đề tồn tại trước khi tải danh sách thí sinh
            var examSetResult = await _repository.GetByIdAsync(examSetId);
            if (!examSetResult.Success || examSetResult.Data == null)
                return (null, new List<ExamSetTakerModel>(), 0, examSetResult.Message ?? "Không tìm thấy bộ đề.");

            var takersResult = await _repository.GetExamTakersAsync(new ExamSetTakerSearchModel
            {
                ExamSetId = examSetId,
                PageIndex = pageIndex,
                PageSize = 15
            });
            var (items, total, error) = ApiListHelper.ParseList(takersResult);
            return (examSetResult.Data, items, total, error);
        }

        /// <summary>Hiển thị tên loại thi trên UI (enum → tiếng Việt).</summary>
        public string GetExamTypeName(int examType) => examType switch
        {
            (int)ExamTypeEnum.Trial => "Thi thử",
            (int)ExamTypeEnum.Real => "Thi thật",
            _ => "—"
        };

        /// <summary>Hiển thị tên trạng thái phiên thi trên UI (enum → tiếng Việt).</summary>
        public string GetSessionStatusName(int status) => status switch
        {
            (int)ExamSessionStatusEnum.InProgress => "Đang thi",
            (int)ExamSessionStatusEnum.Completed => "Hoàn thành",
            (int)ExamSessionStatusEnum.Expired => "Hết giờ",
            _ => "—"
        };
    }
}
