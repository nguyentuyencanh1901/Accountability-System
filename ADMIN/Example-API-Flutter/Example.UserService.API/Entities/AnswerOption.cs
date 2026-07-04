using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class AnswerOption : EntityAuditBase<long>
    {
        public long QuestionId { get; set; }
        public Question Question { get; set; }

        public string Content { get; set; }       // Nội dung đáp án
        public bool IsCorrect { get; set; }       // Đáp án đúng
        public int SortOrder { get; set; }        // Thứ tự hiển thị
    }
}
