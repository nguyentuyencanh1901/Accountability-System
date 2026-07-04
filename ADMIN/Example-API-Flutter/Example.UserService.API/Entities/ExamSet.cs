using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class ExamSet : EntityAuditBase<long>
    {
        public string Name { get; set; }              // Tên bộ đề
        public string Description { get; set; }       // Mô tả
        public int RequiredTotalPoints { get; set; }  // Tổng điểm yêu cầu
        public int QuestionCount { get; set; }        // Số câu hỏi cần lấy
        public int EasyCount { get; set; }            // Số câu dễ
        public int MediumCount { get; set; }        // Số câu trung bình
        public int HardCount { get; set; }            // Số câu khó
        public int Type { get; set; }                 // 1=Thi thật, 2=Thi thử
        public int DurationMinutes { get; set; }      // Thời gian làm bài (phút)
        public int Status { get; set; }               // 1=Active, 2=Inactive
    }
}
