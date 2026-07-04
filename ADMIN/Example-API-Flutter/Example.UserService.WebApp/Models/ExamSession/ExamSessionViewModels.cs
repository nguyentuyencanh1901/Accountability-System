using Example.UserService.API.Models;

namespace Example.UserService.WebApp.Models.ExamSession
{
    public class ExamSessionIndexViewModel : PagedViewModel
    {
        public List<ExamSessionModel> Items { get; set; } = new();
    }

    public class ExamSessionHistoryViewModel : PagedViewModel
    {
        public List<ExamSessionModel> Items { get; set; } = new();
    }

    public class ExamSessionDetailsViewModel
    {
        public ExamSessionModel? Session { get; set; }
        public string? ApiBaseUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class TakeExamViewModel
    {
        public ExamSessionModel? Session { get; set; }
        public int DurationMinutes { get; set; }
        public long EndTimeUnixMs { get; set; }
        public string? ApiBaseUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class QuestionImagePartialModel
    {
        public string? RelativeImageUrl { get; set; }
        public string? ApiBaseUrl { get; set; }
        public string AltText { get; set; } = "Ảnh minh họa câu hỏi";
        /// <summary>take | result</summary>
        public string Variant { get; set; } = "take";
    }

    public class SubmitExamFormModel
    {
        public long ExamSessionId { get; set; }
        /// <summary>Số lần vi phạm quy chế — gửi khi hủy bài tự động.</summary>
        public int ViolationCount { get; set; }
        public List<SubmitAnswerFormItem> Answers { get; set; } = new();
    }

    public class SubmitAnswerFormItem
    {
        public long QuestionId { get; set; }
        public List<long> SelectedAnswerOptionIds { get; set; } = new();
    }
}
