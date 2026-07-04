using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Helpers;
using Example.UserService.WebAdmin.Models.AnswerOption;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;
using Mapster;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho đáp án câu hỏi: danh sách theo questionId (validate ngữ cảnh câu hỏi),
    /// ApiListHelper, nạp thông tin lĩnh vực từ API liên quan, map ViewModel ↔ SaveModel.
    /// </summary>
    public class AnswerOptionService : IAnswerOptionService
    {
        // Repository ủy quyền gọi HTTP API đáp án
        private readonly IAnswerOptionRepository _repository;
        // Repository câu hỏi — dùng validate ngữ cảnh và nạp thông tin câu hỏi/lĩnh vực
        private readonly IQuestionRepository _questionRepository;

        /// <summary>Khởi tạo service với repository đáp án và câu hỏi.</summary>
        public AnswerOptionService(
            IAnswerOptionRepository repository,
            IQuestionRepository questionRepository)
        {
            _repository = repository;
            _questionRepository = questionRepository;
        }

        /// <summary>
        /// Danh sách đáp án theo questionId: validate câu hỏi tồn tại, ApiListHelper.ParseList,
        /// trả kèm thông tin lĩnh vực và nội dung câu hỏi cho tiêu đề trang.
        /// </summary>
        public async Task<(List<AnswerOptionModel> Items, long TotalItems, long FieldId, string FieldName, string QuestionContent, string? ErrorMessage, bool InvalidQuestion)> GetListPagingAsync(long questionId, int pageIndex)
        {
            if (questionId <= 0)
                return (new List<AnswerOptionModel>(), 0, 0, string.Empty, string.Empty, null, true);

            var questionContext = await LoadQuestionContextAsync(questionId);
            if (questionContext == null)
                return (new List<AnswerOptionModel>(), 0, 0, string.Empty, string.Empty, "Không tìm thấy câu hỏi.", true);

            var result = await _repository.GetListAsync(new AnswerOptionSearchModel
            {
                PageIndex = pageIndex,
                PageSize = 10,
                QuestionId = questionId
            });
            var (items, total, error) = ApiListHelper.ParseList(result);
            return (items, total, questionContext.Value.FieldId, questionContext.Value.FieldName, questionContext.Value.QuestionContent, error, false);
        }

        /// <summary>Tạo form thêm mới gắn với câu hỏi; trả InvalidQuestion nếu questionId không hợp lệ.</summary>
        public async Task<(AnswerOptionFormViewModel? Model, string? ErrorMessage, bool InvalidQuestion)> BuildCreateFormAsync(long questionId)
        {
            if (questionId <= 0)
                return (null, null, true);

            var questionContext = await LoadQuestionContextAsync(questionId);
            if (questionContext == null)
                return (null, "Không tìm thấy câu hỏi.", true);

            return (new AnswerOptionFormViewModel
            {
                QuestionId = questionId,
                FieldId = questionContext.Value.FieldId,
                FieldName = questionContext.Value.FieldName,
                QuestionContent = questionContext.Value.QuestionContent
            }, null, false);
        }

        /// <summary>Lấy form sửa: map API → ViewModel (Mapster), bổ sung ngữ cảnh câu hỏi/lĩnh vực.</summary>
        public async Task<(AnswerOptionFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy đáp án.");

            var model = result.Data.Adapt<AnswerOptionFormViewModel>();
            var questionContext = await LoadQuestionContextAsync(model.QuestionId);
            if (questionContext != null)
            {
                model.FieldId = questionContext.Value.FieldId;
                model.FieldName = questionContext.Value.FieldName;
                model.QuestionContent = questionContext.Value.QuestionContent;
            }

            return (model, null);
        }

        /// <summary>Map ViewModel → SaveModel và gọi API thêm.</summary>
        public Task<ResponseData<object>> InsertAsync(AnswerOptionFormViewModel model)
            => _repository.AddAsync(model.Adapt<AnswerOptionSaveModel>());

        /// <summary>Map ViewModel → SaveModel và gọi API cập nhật.</summary>
        public Task<ResponseData<object>> UpdateAsync(AnswerOptionFormViewModel model)
            => _repository.UpdateAsync(model.Adapt<AnswerOptionSaveModel>());

        /// <summary>Ủy quyền xóa qua repository.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id)
            => _repository.DeleteAsync(id);

        /// <summary>Tải ngữ cảnh câu hỏi (FieldId, tên lĩnh vực, nội dung rút gọn) phục vụ form/danh sách.</summary>
        public async Task<(long FieldId, string FieldName, string QuestionContent)?> LoadQuestionContextAsync(long questionId)
        {
            var questionResult = await _questionRepository.GetByIdAsync(questionId);
            if (!questionResult.Success || questionResult.Data == null)
                return null;

            var question = questionResult.Data;
            // Rút gọn nội dung câu hỏi dài hơn 80 ký tự cho hiển thị tiêu đề
            var content = question.Content?.Length > 80
                ? question.Content[..80] + "..."
                : question.Content ?? string.Empty;

            return (question.FieldId, question.FieldName ?? string.Empty, content);
        }
    }
}
