namespace Example.UserService.API.Models
{
    public class StartExamModel
    {
        public long ExamPeriodAssignmentId { get; set; }
        public long ExamSetId { get; set; }
        public int ExamType { get; set; }             // 1=Thi thử, 2=Thi thật
    }
}
