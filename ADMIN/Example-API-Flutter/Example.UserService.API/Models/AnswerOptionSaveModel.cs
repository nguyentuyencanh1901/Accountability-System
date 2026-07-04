using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class AnswerOptionModel : AnswerOptionSaveModel
    {
        public string QuestionContent { get; set; }
    }

    public class AnswerOptionSaveModel
    {
        public long Id { get; set; }
        public long QuestionId { get; set; }
        public string Content { get; set; }       // Nội dung đáp án
        public bool IsCorrect { get; set; }       // Đáp án đúng
        public int SortOrder { get; set; }        // Thứ tự hiển thị
    }

    public class AnswerOptionSearchModel : BaseSearch
    {
        public long? QuestionId { get; set; }
    }
}
