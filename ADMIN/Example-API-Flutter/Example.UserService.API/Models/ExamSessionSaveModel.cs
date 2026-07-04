using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class ExamSessionModel : ExamSessionSaveModel
    {
        public string ExamSetName { get; set; }
        public string UserFullName { get; set; }
        public List<ExamSessionQuestionModel> Questions { get; set; }
        public List<ExamSessionAnswerModel> Answers { get; set; }
    }

    public class ExamSessionSaveModel
    {
        public long Id { get; set; }
        public long ExamSetId { get; set; }
        public long UserId { get; set; }
        public long? ExamPeriodAssignmentId { get; set; }
        public int ExamType { get; set; }             // 1=Thi thử, 2=Thi thật
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public decimal TotalScore { get; set; }
        public decimal MaxScore { get; set; }
        public int Status { get; set; }               // 1=Đang thi, 2=Hoàn thành, 3=Hết hạn, 4=Bị hủy bỏ
        /// <summary>Số lần vi phạm quy chế thi khi bị hủy.</summary>
        public int ViolationCount { get; set; }
    }

    public class ExamSessionSearchModel : BaseSearch
    {
        public long? ExamSetId { get; set; }
        public long? UserId { get; set; }
        public int? ExamType { get; set; }
        public int? Status { get; set; }
        public bool HistoryOnly { get; set; }
    }
}
