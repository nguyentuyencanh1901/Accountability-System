using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class QuestionModel : QuestionSaveModel
    {
        public string? FieldName { get; set; }
    }

    public class QuestionSaveModel
    {
        public long Id { get; set; }
        public long FieldId { get; set; }
        public string Content { get; set; }       // Nội dung câu hỏi
        public string? ImageUrl { get; set; }     // Đường dẫn ảnh đã lưu
        public string? ImageBase64 { get; set; }  // Ảnh mới (base64) từ form upload
        public string? ImageFileName { get; set; }
        public bool RemoveImage { get; set; }
        public int Points { get; set; }           // Điểm câu hỏi
        public int QuestionType { get; set; }     // 1=Chọn 1 đáp án, 2=Chọn nhiều đáp án
        public int DifficultyLevel { get; set; }  // 1=Dễ, 2=Trung bình, 3=Khó
        public int Status { get; set; }           // 1=Active, 2=Inactive

        public List<AnswerOptionSaveModel> AnswerOptions { get; set; }
    }

    public class QuestionSearchModel : BaseSearch
    {
        public long? FieldId { get; set; }
        public int? QuestionType { get; set; }
        public int? DifficultyLevel { get; set; }
    }
}
