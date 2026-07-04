using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Helpers;
using Example.UserService.WebAdmin.Models.ExamSessionQuestion;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho câu hỏi trong bài thi: phân trang có lọc, ApiListHelper,
    /// nạp dropdown phiên thi/câu hỏi, map ViewModel ↔ SaveModel.
    /// </summary>
    public class ExamSessionQuestionService : IExamSessionQuestionService
    {
        // Repository ủy quyền gọi HTTP API câu hỏi trong bài thi
        private readonly IExamSessionQuestionRepository _repository;
        // Repository phiên thi — dùng nạp dropdown lọc và form
        private readonly IExamSessionRepository _examSessionRepository;
        // Repository câu hỏi — dùng nạp dropdown lọc và form
        private readonly IQuestionRepository _questionRepository;

        /// <summary>Khởi tạo service với các repository liên quan.</summary>
        public ExamSessionQuestionService(
            IExamSessionQuestionRepository repository,
            IExamSessionRepository examSessionRepository,
            IQuestionRepository questionRepository)
        {
            _repository = repository;
            _examSessionRepository = examSessionRepository;
            _questionRepository = questionRepository;
        }

        /// <summary>
        /// Danh sách phân trang với bộ lọc ExamSessionId/QuestionId;
        /// ApiListHelper.ParseList + dropdown lọc có mục "Tất cả".
        /// </summary>
        public async Task<(List<ExamSessionQuestionModel> Items, long TotalItems, string? ErrorMessage, List<SelectListItem> ExamSessionFilterOptions, List<SelectListItem> QuestionFilterOptions)> GetListPagingAsync(int pageIndex, long? examSessionId, long? questionId)
        {
            var result = await _repository.GetListAsync(new ExamSessionQuestionSearchModel
            {
                PageIndex = pageIndex,
                PageSize = 10,
                ExamSessionId = examSessionId,
                QuestionId = questionId
            });
            var (items, total, error) = ApiListHelper.ParseList(result);
            var examSessionFilterOptions = await LoadExamSessionFilterOptionsAsync(examSessionId);
            var questionFilterOptions = await LoadQuestionFilterOptionsAsync(questionId);
            return (items, total, error, examSessionFilterOptions, questionFilterOptions);
        }

        /// <summary>Tạo form thêm mới và nạp dropdown.</summary>
        public async Task<ExamSessionQuestionFormViewModel> BuildCreateFormAsync()
        {
            var model = new ExamSessionQuestionFormViewModel();
            await PopulateFormOptionsAsync(model);
            return model;
        }

        /// <summary>Lấy form sửa: map API → ViewModel (Mapster), nạp dropdown.</summary>
        public async Task<(ExamSessionQuestionFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy câu hỏi bài thi.");

            var model = result.Data.Adapt<ExamSessionQuestionFormViewModel>();
            await PopulateFormOptionsAsync(model);
            return (model, null);
        }

        /// <summary>Map ViewModel → SaveModel và gọi API thêm.</summary>
        public Task<ResponseData<object>> InsertAsync(ExamSessionQuestionFormViewModel model)
            => _repository.AddAsync(model.Adapt<ExamSessionQuestionSaveModel>());

        /// <summary>Map ViewModel → SaveModel và gọi API cập nhật.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamSessionQuestionFormViewModel model)
            => _repository.UpdateAsync(model.Adapt<ExamSessionQuestionSaveModel>());

        /// <summary>Ủy quyền xóa qua repository.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id)
            => _repository.DeleteAsync(id);

        /// <summary>Nạp dropdown phiên thi và câu hỏi cho form.</summary>
        public async Task PopulateFormOptionsAsync(ExamSessionQuestionFormViewModel model)
        {
            model.ExamSessionOptions = await LoadExamSessionOptionsAsync(model.ExamSessionId);
            model.QuestionOptions = await LoadQuestionOptionsAsync(model.QuestionId);
        }

        /// <summary>Tải danh sách phiên thi làm SelectListItem.</summary>
        public async Task<List<SelectListItem>> LoadExamSessionOptionsAsync(long selectedId)
        {
            var result = await _examSessionRepository.GetListAsync(new ExamSessionSearchModel { PageIndex = 1, PageSize = 500 });
            return result.Success && result.Data != null
                ? result.Data.Select(e => new SelectListItem(
                    FormatExamSessionLabel(e), e.Id.ToString(), e.Id == selectedId)).ToList()
                : new List<SelectListItem>();
        }

        /// <summary>Dropdown lọc phiên thi: thêm mục "Tất cả" ở đầu.</summary>
        public async Task<List<SelectListItem>> LoadExamSessionFilterOptionsAsync(long? selectedId)
        {
            var options = await LoadExamSessionOptionsAsync(selectedId ?? 0);
            options.Insert(0, new SelectListItem("Tất cả", "", !selectedId.HasValue));
            return options;
        }

        /// <summary>Tải danh sách câu hỏi làm SelectListItem (nhãn rút gọn).</summary>
        public async Task<List<SelectListItem>> LoadQuestionOptionsAsync(long selectedId)
        {
            var result = await _questionRepository.GetListAsync(new QuestionSearchModel { PageIndex = 1, PageSize = 500 });
            return result.Success && result.Data != null
                ? result.Data.Select(q => new SelectListItem(
                    FormatQuestionLabel(q), q.Id.ToString(), q.Id == selectedId)).ToList()
                : new List<SelectListItem>();
        }

        /// <summary>Dropdown lọc câu hỏi: thêm mục "Tất cả" ở đầu.</summary>
        public async Task<List<SelectListItem>> LoadQuestionFilterOptionsAsync(long? selectedId)
        {
            var options = await LoadQuestionOptionsAsync(selectedId ?? 0);
            options.Insert(0, new SelectListItem("Tất cả", "", !selectedId.HasValue));
            return options;
        }

        /// <summary>Định dạng nhãn phiên thi (id + thí sinh + bộ đề).</summary>
        public string FormatExamSessionLabel(ExamSessionModel e)
            => $"#{e.Id} - {e.UserFullName} / {e.ExamSetName}";

        /// <summary>Định dạng nhãn câu hỏi (cắt ngắn nội dung &gt; 50 ký tự).</summary>
        public string FormatQuestionLabel(QuestionModel q)
        {
            var content = q.Content?.Length > 50 ? q.Content[..50] + "..." : q.Content;
            return $"#{q.Id} - {content}";
        }
    }
}
