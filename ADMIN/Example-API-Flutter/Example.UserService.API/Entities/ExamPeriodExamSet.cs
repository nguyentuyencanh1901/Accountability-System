using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class ExamPeriodExamSet : EntityAuditBase<long>
    {
        public long ExamPeriodId { get; set; }
        public ExamPeriod ExamPeriod { get; set; }

        public long ExamSetId { get; set; }
        public ExamSet ExamSet { get; set; }
    }
}
