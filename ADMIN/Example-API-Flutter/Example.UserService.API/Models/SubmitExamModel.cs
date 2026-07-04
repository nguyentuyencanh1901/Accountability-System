namespace Example.UserService.API.Models
{
    public class SubmitExamModel
    {
        public long ExamSessionId { get; set; }
        public List<SubmitAnswerModel> Answers { get; set; }
        /// <summary>Số lần vi phạm quy chế — chỉ dùng khi hủy bài (cancel-exam-violation).</summary>
        public int ViolationCount { get; set; }
    }

    public class SubmitAnswerModel
    {
        public long QuestionId { get; set; }
        public List<long> SelectedAnswerOptionIds { get; set; }
    }
}
