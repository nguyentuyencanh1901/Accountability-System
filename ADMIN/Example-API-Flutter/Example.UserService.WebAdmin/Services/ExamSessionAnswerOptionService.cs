using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Helpers;
using Example.UserService.WebAdmin.Models.ExamSessionAnswerOption;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho đáp án đã chọn trong bài thi: phân trang có lọc, ApiListHelper,
    /// nạp dropdown câu trả lời/đáp án, map ViewModel ↔ SaveModel.
    /// </summary>
    public class ExamSessionAnswerOptionService : IExamSessionAnswerOptionService
    {
        // Repository ủy quyền gọi HTTP API đáp án đã chọn trong bài thi
        private readonly IExamSessionAnswerOptionRepository _repository;
        // Repository câu trả lời — dùng nạp dropdown lọc và form
        private readonly IExamSessionAnswerRepository _examSessionAnswerRepository;
        // Repository đáp án — dùng nạp dropdown lọc và form
        private readonly IAnswerOptionRepository _answerOptionRepository;

        /// <summary>Khởi tạo service với các repository liên quan.</summary>
        public ExamSessionAnswerOptionService(
            IExamSessionAnswerOptionRepository repository,
            IExamSessionAnswerRepository examSessionAnswerRepository,
            IAnswerOptionRepository answerOptionRepository)
        {
            _repository = repository;
            _examSessionAnswerRepository = examSessionAnswerRepository;
            _answerOptionRepository = answerOptionRepository;
        }

        /// <summary>
        /// Danh sách phân trang với bộ lọc ExamSessionAnswerId/AnswerOptionId;
        /// ApiListHelper.ParseList + nạp dropdown lọc kèm mục "Tất cả".
        /// </summary>
        public async Task<(List<ExamSessionAnswerOptionModel> Items, long TotalItems, string? ErrorMessage, List<SelectListItem> ExamSessionAnswerFilterOptions, List<SelectListItem> AnswerOptionFilterOptions)> GetListPagingAsync(int pageIndex, long? examSessionAnswerId, long? answerOptionId)
        {
            var result = await _repository.GetListAsync(new ExamSessionAnswerOptionSearchModel
            {
                PageIndex = pageIndex,
                PageSize = 10,
                ExamSessionAnswerId = examSessionAnswerId,
                AnswerOptionId = answerOptionId
            });
            var (items, total, error) = ApiListHelper.ParseList(result);
            var examSessionAnswerFilterOptions = await LoadExamSessionAnswerFilterOptionsAsync(examSessionAnswerId);
            var answerOptionFilterOptions = await LoadAnswerOptionFilterOptionsAsync(answerOptionId);
            return (items, total, error, examSessionAnswerFilterOptions, answerOptionFilterOptions);
        }

        /// <summary>Tạo form thêm mới và nạp dropdown.</summary>
        public async Task<ExamSessionAnswerOptionFormViewModel> BuildCreateFormAsync()
        {
            var model = new ExamSessionAnswerOptionFormViewModel();
            await PopulateFormOptionsAsync(model);
            return model;
        }

        /// <summary>Lấy form sửa: map API → ViewModel (Mapster), nạp dropdown.</summary>
        public async Task<(ExamSessionAnswerOptionFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy đáp án đã chọn.");

            var model = result.Data.Adapt<ExamSessionAnswerOptionFormViewModel>();
            await PopulateFormOptionsAsync(model);
            return (model, null);
        }

        /// <summary>Map ViewModel → SaveModel và gọi API thêm.</summary>
        public Task<ResponseData<object>> InsertAsync(ExamSessionAnswerOptionFormViewModel model)
            => _repository.AddAsync(model.Adapt<ExamSessionAnswerOptionSaveModel>());

        /// <summary>Map ViewModel → SaveModel và gọi API cập nhật.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamSessionAnswerOptionFormViewModel model)
            => _repository.UpdateAsync(model.Adapt<ExamSessionAnswerOptionSaveModel>());

        /// <summary>Ủy quyền xóa qua repository.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id)
            => _repository.DeleteAsync(id);

        /// <summary>Nạp dropdown câu trả lời bài thi và đáp án cho form.</summary>
        public async Task PopulateFormOptionsAsync(ExamSessionAnswerOptionFormViewModel model)
        {
            model.ExamSessionAnswerOptions = await LoadExamSessionAnswerOptionsAsync(model.ExamSessionAnswerId);
            model.AnswerOptionOptions = await LoadAnswerOptionOptionsAsync(model.AnswerOptionId);
        }

        /// <summary>Tải danh sách câu trả lời bài thi làm SelectListItem (nhãn rút gọn).</summary>
        public async Task<List<SelectListItem>> LoadExamSessionAnswerOptionsAsync(long selectedId)
        {
            var result = await _examSessionAnswerRepository.GetListAsync(new ExamSessionAnswerSearchModel { PageIndex = 1, PageSize = 500 });
            return result.Success && result.Data != null
                ? result.Data.Select(a => new SelectListItem(
                    FormatExamSessionAnswerLabel(a), a.Id.ToString(), a.Id == selectedId)).ToList()
                : new List<SelectListItem>();
        }

        /// <summary>Dropdown lọc danh sách: thêm mục "Tất cả" ở đầu khi chưa chọn bộ lọc.</summary>
        public async Task<List<SelectListItem>> LoadExamSessionAnswerFilterOptionsAsync(long? selectedId)
        {
            var options = await LoadExamSessionAnswerOptionsAsync(selectedId ?? 0);
            options.Insert(0, new SelectListItem("Tất cả", "", !selectedId.HasValue));
            return options;
        }

        /// <summary>Tải danh sách đáp án làm SelectListItem (nhãn rút gọn nội dung).</summary>
        public async Task<List<SelectListItem>> LoadAnswerOptionOptionsAsync(long selectedId)
        {
            var result = await _answerOptionRepository.GetListAsync(new AnswerOptionSearchModel { PageIndex = 1, PageSize = 500 });
            return result.Success && result.Data != null
                ? result.Data.Select(a => new SelectListItem(
                    FormatAnswerOptionLabel(a), a.Id.ToString(), a.Id == selectedId)).ToList()
                : new List<SelectListItem>();
        }

        /// <summary>Dropdown lọc đáp án: thêm mục "Tất cả" ở đầu.</summary>
        public async Task<List<SelectListItem>> LoadAnswerOptionFilterOptionsAsync(long? selectedId)
        {
            var options = await LoadAnswerOptionOptionsAsync(selectedId ?? 0);
            options.Insert(0, new SelectListItem("Tất cả", "", !selectedId.HasValue));
            return options;
        }

        /// <summary>Định dạng nhãn hiển thị câu trả lời (cắt ngắn nội dung câu hỏi &gt; 40 ký tự).</summary>
        public string FormatExamSessionAnswerLabel(ExamSessionAnswerModel a)
        {
            var question = a.QuestionContent?.Length > 40 ? a.QuestionContent[..40] + "..." : a.QuestionContent;
            return $"#{a.Id} - {a.ExamSessionInfo} / {question}";
        }

        /// <summary>Định dạng nhãn hiển thị đáp án (cắt ngắn nội dung &gt; 50 ký tự).</summary>
        public string FormatAnswerOptionLabel(AnswerOptionModel a)
        {
            var content = a.Content?.Length > 50 ? a.Content[..50] + "..." : a.Content;
            return $"#{a.Id} - {content}";
        }
    }
}
