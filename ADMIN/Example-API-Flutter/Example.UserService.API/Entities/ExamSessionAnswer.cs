using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class ExamSessionAnswer : EntityAuditBase<long>
    {
        public long ExamSessionId { get; set; }
        public ExamSession ExamSession { get; set; }

        public long QuestionId { get; set; }
        public Question Question { get; set; }

        public bool IsCorrect { get; set; }           // Trả lời đúng/sai
        public decimal Score { get; set; }            // Điểm đạt được
    }
}
