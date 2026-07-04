using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class Question : EntityAuditBase<long>
    {
        public long FieldId { get; set; }           // Thuộc lĩnh vực
        public string Content { get; set; }       // Nội dung câu hỏi
        public string? ImageUrl { get; set; }     // Đường dẫn ảnh (relative: questions/{id}/file.jpg)
        public int Points { get; set; }           // Điểm câu hỏi
        public int QuestionType { get; set; }     // 1=Chọn 1 đáp án, 2=Chọn nhiều đáp án
        public int DifficultyLevel { get; set; }  // 1=Dễ, 2=Trung bình, 3=Khó
        public int Status { get; set; }           // 1=Active, 2=Inactive
    }
}
