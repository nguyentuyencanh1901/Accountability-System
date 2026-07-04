using Example.API.Client.Configuration;
using Example.Common.Models;
using Example.Common.Utilities.Helper;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Helpers;
using Example.UserService.WebAdmin.Models.Question;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;
using Mapster;
using Microsoft.Extensions.Options;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho câu hỏi: phân trang có lọc (ApiListHelper), form CRUD kèm đáp án,
    /// upload ảnh base64, nạp dropdown lĩnh vực/loại/độ khó, map ViewModel ↔ SaveModel.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        // Repository ủy quyền gọi HTTP API câu hỏi
        private readonly IQuestionRepository _repository;
        // Repository lĩnh vực — dùng nạp dropdown lọc và form
        private readonly IFieldRepository _fieldRepository;
        // Base URL API — ghép URL ảnh công khai cho preview
        private readonly string _apiBaseUrl;

        /// <summary>Khởi tạo service với repository và cấu hình API base URL.</summary>
        public QuestionService(
            IQuestionRepository repository,
            IFieldRepository fieldRepository,
            IOptions<ApiClientOptions> apiOptions)
        {
            _repository = repository;
            _fieldRepository = fieldRepository;
            _apiBaseUrl = apiOptions.Value.BaseUrl ?? string.Empty;
        }

        /// <summary>
        /// Danh sách phân trang với bộ lọc lĩnh vực/loại/độ khó/từ khóa;
        /// ApiListHelper.ParseList tách items/total/error, nạp thêm dropdown lọc.
        /// </summary>
        public async Task<QuestionIndexViewModel> GetListPagingAsync(QuestionIndexViewModel filter)
        {
            var result = await _repository.GetListAsync(new QuestionSearchModel
            {
                PageIndex = filter.PageIndex,
                PageSize = 10,
                FieldId = filter.FieldIdFilter,
                QuestionType = filter.QuestionTypeFilter,
                DifficultyLevel = filter.DifficultyLevelFilter,
                Keyword = filter.Keyword
            });
            var (items, total, error) = ApiListHelper.ParseList(result);
            filter.Items = items;
            filter.TotalItems = total;
            filter.ErrorMessage = error;
            filter.ApiBaseUrl = _apiBaseUrl;
            await PopulateIndexFiltersAsync(filter);
            return filter;
        }

        /// <summary>Tạo form thêm mới: khởi tạo tối thiểu 2 đáp án và nạp dropdown lĩnh vực.</summary>
        public async Task<QuestionFormViewModel> BuildCreateFormAsync()
        {
            var model = BuildFormViewModel(new QuestionFormViewModel());
            await PopulateDropdownOptionsAsync(model);
            return model;
        }

        /// <summary>
        /// Lấy form sửa: map API → ViewModel (Mapster), build URL preview ảnh, nạp dropdown.
        /// </summary>
        public async Task<(QuestionFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy câu hỏi.");

            var model = result.Data.Adapt<QuestionFormViewModel>();
            // Ghép base URL API với đường dẫn ảnh lưu trên server
            model.ImagePreviewUrl = EntityImageHelper.BuildPublicUrl(model.ImageUrl, _apiBaseUrl);
            var viewModel = BuildFormViewModel(model);
            await PopulateDropdownOptionsAsync(viewModel);
            return (viewModel, null);
        }

        /// <summary>Nạp dropdown lĩnh vực (chỉ lĩnh vực Active) cho form tạo/sửa.</summary>
        public async Task PopulateDropdownOptionsAsync(QuestionFormViewModel model)
        {
            var fieldResult = await _fieldRepository.GetListAsync(new FieldSearchModel { PageIndex = 1, PageSize = 500, Status = 1 });
            var fields = fieldResult.Success && fieldResult.Data != null ? fieldResult.Data : new List<FieldModel>();
            model.FieldOptions = fields
                .Select(f => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(f.Name, f.Id.ToString(), f.Id == model.FieldId))
                .ToList();
        }

        /// <summary>Map ViewModel → SaveModel (có xử lý ảnh) và gọi API thêm.</summary>
        public Task<ResponseData<object>> InsertAsync(QuestionFormViewModel model)
            => _repository.AddAsync(ToSaveModel(model));

        /// <summary>Map ViewModel → SaveModel (có xử lý ảnh) và gọi API cập nhật.</summary>
        public Task<ResponseData<object>> UpdateAsync(QuestionFormViewModel model)
            => _repository.UpdateAsync(ToSaveModel(model));

        /// <summary>Cập nhật nhanh trạng thái trên danh sách (Index).</summary>
        public Task<ResponseData<object>> UpdateStatusAsync(long id, int status)
            => _repository.UpdateStatusAsync(new UpdateStatusModel { Id = id, Status = status });

        /// <summary>Ủy quyền xóa qua repository.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id)
            => _repository.DeleteAsync(id);

        /// <summary>Nạp dropdown loại câu hỏi, độ khó và trạng thái cho form.</summary>
        public void PopulateFormOptions(QuestionFormViewModel model)
        {
            model.QuestionTypeOptions = SelectListHelper.QuestionTypes(model.QuestionType);
            model.DifficultyLevelOptions = SelectListHelper.DifficultyLevels(model.DifficultyLevel);
            model.StatusOptions = SelectListHelper.Statuses(model.Status);
        }

        /// <summary>Cập nhật URL preview ảnh từ ImageUrl và base URL API.</summary>
        public void ApplyImagePreview(QuestionFormViewModel model)
        {
            model.ImagePreviewUrl = EntityImageHelper.BuildPublicUrl(model.ImageUrl, _apiBaseUrl);
        }

        /// <summary>
        /// Dựng ViewModel form: nạp dropdown, đảm bảo tối thiểu 2 đáp án, chuẩn hóa SortOrder.
        /// </summary>
        public QuestionFormViewModel BuildFormViewModel(QuestionFormViewModel model)
        {
            PopulateFormOptions(model);
            // Form luôn có ít nhất 2 ô đáp án trống
            if (model.AnswerOptions.Count < 2)
            {
                model.AnswerOptions = Enumerable.Range(1, 2)
                    .Select(i => new AnswerOptionFormViewModel { SortOrder = i }).ToList();
            }
            // Gán thứ tự mặc định nếu chưa có
            for (var i = 0; i < model.AnswerOptions.Count; i++)
            {
                if (model.AnswerOptions[i].SortOrder <= 0)
                    model.AnswerOptions[i].SortOrder = i + 1;
            }
            return model;
        }

        /// <summary>Nạp dropdown lọc lĩnh vực, loại câu hỏi và độ khó cho trang danh sách.</summary>
        private async Task PopulateIndexFiltersAsync(QuestionIndexViewModel filter)
        {
            var fieldResult = await _fieldRepository.GetListAsync(new FieldSearchModel { PageIndex = 1, PageSize = 500 });
            var fields = fieldResult.Success && fieldResult.Data != null ? fieldResult.Data : new List<FieldModel>();
            filter.FieldFilterOptions = SelectListHelper.AllOption("Tất cả lĩnh vực")
                .Concat(fields.Select(f => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(
                    f.Name, f.Id.ToString(), filter.FieldIdFilter == f.Id)))
                .ToList();

            // Mục "Tất cả" + danh sách loại câu hỏi
            filter.QuestionTypeFilterOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
            {
                new("Tất cả loại", "", !filter.QuestionTypeFilter.HasValue)
            };
            filter.QuestionTypeFilterOptions.AddRange(
                SelectListHelper.QuestionTypes(filter.QuestionTypeFilter ?? (int)Example.Common.Enums.QuestionTypeEnum.SingleChoice)
                    .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(
                        x.Text, x.Value, filter.QuestionTypeFilter.HasValue && x.Value == filter.QuestionTypeFilter.Value.ToString())));

            // Mục "Tất cả" + danh sách độ khó
            filter.DifficultyFilterOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
            {
                new("Tất cả độ khó", "", !filter.DifficultyLevelFilter.HasValue)
            };
            filter.DifficultyFilterOptions.AddRange(
                SelectListHelper.DifficultyLevels(filter.DifficultyLevelFilter ?? (int)Example.Common.Enums.DifficultyLevelEnum.Medium)
                    .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(
                        x.Text, x.Value, filter.DifficultyLevelFilter.HasValue && x.Value == filter.DifficultyLevelFilter.Value.ToString())));
        }

        /// <summary>
        /// Map ViewModel → SaveModel: Mapster + lọc đáp án rỗng, chuẩn hóa SortOrder,
        /// chuyển file ảnh upload sang base64 nếu có.
        /// </summary>
        private QuestionSaveModel ToSaveModel(QuestionFormViewModel model)
        {
            var saveModel = model.Adapt<QuestionSaveModel>();
            // Chỉ gửi đáp án có nội dung; gán SortOrder nếu thiếu
            saveModel.AnswerOptions = model.AnswerOptions
                .Where(a => !string.IsNullOrWhiteSpace(a.Content))
                .Select((a, index) =>
                {
                    a.SortOrder = a.SortOrder > 0 ? a.SortOrder : index + 1;
                    return a;
                })
                .Adapt<List<AnswerOptionSaveModel>>();

            // Upload ảnh mới: đọc file → base64 gửi kèm API
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                model.ImageFile.CopyTo(memoryStream);
                saveModel.ImageBase64 = Convert.ToBase64String(memoryStream.ToArray());
                saveModel.ImageFileName = model.ImageFile.FileName;
            }

            return saveModel;
        }
    }
}
