using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class ExamPeriodAssignment : EntityAuditBase<long>
    {
        public long ExamPeriodId { get; set; }
        public ExamPeriod ExamPeriod { get; set; }

        public long UserId { get; set; }
        public AppUser User { get; set; }

        public long? ExamSetId { get; set; }
        public ExamSet? ExamSet { get; set; }

        public int ExamType { get; set; }             // 1=Thi thử, 2=Thi thật
        public int Status { get; set; }               // 1=Đã phân công, 2=Đang thi, 3=Hoàn thành, 4=Vắng thi
        public long? ExamSessionId { get; set; }
        public ExamSession ExamSession { get; set; }
    }
}
