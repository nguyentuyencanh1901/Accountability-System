using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class ExamPeriodAssignmentModel : ExamPeriodAssignmentSaveModel
    {
        public string ExamPeriodName { get; set; }
        public string UserFullName { get; set; }
        public string ExamSetName { get; set; }
        public int ExamPeriodStatus { get; set; }
        public DateTimeOffset? ExamPeriodStartAt { get; set; }
        public DateTimeOffset? ExamPeriodEndAt { get; set; }
        public decimal? SessionTotalScore { get; set; }
        public decimal? SessionMaxScore { get; set; }
        public DateTimeOffset? SessionStartedAt { get; set; }
        public DateTimeOffset? SessionFinishedAt { get; set; }
        public int? SessionStatus { get; set; }
    }

    public class ExamPeriodAssignmentSaveModel
    {
        public long Id { get; set; }
        public long ExamPeriodId { get; set; }
        public long UserId { get; set; }
        public long? ExamSetId { get; set; }
        public int ExamType { get; set; }             // 1=Thi thử, 2=Thi thật
        public int Status { get; set; }               // 1=Đã phân công, 2=Đang thi, 3=Hoàn thành, 4=Vắng thi
        public long? ExamSessionId { get; set; }
    }

    public class ExamPeriodAssignmentSearchModel : BaseSearch
    {
        public long? ExamPeriodId { get; set; }
        public long? UserId { get; set; }
        public long? ExamSetId { get; set; }
        public int? ExamType { get; set; }
    }

    public class ExamPeriodAssignmentBulkSaveModel
    {
        public long ExamPeriodId { get; set; }
        public long UserId { get; set; }
        public List<long> ExamSetIds { get; set; } = new();
        public int ExamType { get; set; }
        public int Status { get; set; }
    }
}
