using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class ExamSession : EntityAuditBase<long>
    {
        public long ExamSetId { get; set; }
        public ExamSet ExamSet { get; set; }

        public long UserId { get; set; }
        public AppUser User { get; set; }

        public long? ExamPeriodAssignmentId { get; set; }
        public ExamPeriodAssignment ExamPeriodAssignment { get; set; }

        public int ExamType { get; set; }             // 1=Thi thử, 2=Thi thật
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public decimal TotalScore { get; set; }       // Điểm đạt được
        public decimal MaxScore { get; set; }         // Tổng điểm tối đa
        public int Status { get; set; }               // 1=Đang thi, 2=Hoàn thành, 3=Hết hạn, 4=Bị hủy bỏ
        /// <summary>Số lần vi phạm quy chế thi (chuyển tab/rời màn hình) khi bị hủy.</summary>
        public int ViolationCount { get; set; }
    }
}
