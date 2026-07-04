using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class ExamSetTakerModel
    {
        public long ExamSessionId { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int ExamType { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public decimal TotalScore { get; set; }
        public decimal MaxScore { get; set; }
        public int Status { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int SkippedCount { get; set; }
    }

    public class ExamSetTakerSearchModel : BaseSearch
    {
        public long ExamSetId { get; set; }
    }
}
