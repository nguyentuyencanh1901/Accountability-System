using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class ExamSessionQuestion : EntityAuditBase<long>
    {
        public long ExamSessionId { get; set; }
        public ExamSession ExamSession { get; set; }

        public long QuestionId { get; set; }
        public Question Question { get; set; }

        public int Points { get; set; }               // Điểm câu hỏi tại thời điểm thi
        public int SortOrder { get; set; }            // Thứ tự câu hỏi
    }
}
