using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class ExamSessionQuestionModel : ExamSessionQuestionSaveModel
    {
        public string Content { get; set; }
        public int QuestionType { get; set; }
        public string? ImageUrl { get; set; }
        public List<AnswerOptionSaveModel> AnswerOptions { get; set; }
        public string ExamSessionInfo { get; set; }
        public bool? IsAnswerCorrect { get; set; }
        public decimal? AnswerScore { get; set; }
        public List<long> SelectedAnswerOptionIds { get; set; } = new();
    }

    public class ExamSessionQuestionSaveModel
    {
        public long Id { get; set; }
        public long ExamSessionId { get; set; }
        public long QuestionId { get; set; }
        public int Points { get; set; }
        public int SortOrder { get; set; }
    }

    public class ExamSessionQuestionSearchModel : BaseSearch
    {
        public long? ExamSessionId { get; set; }
        public long? QuestionId { get; set; }
    }
}
