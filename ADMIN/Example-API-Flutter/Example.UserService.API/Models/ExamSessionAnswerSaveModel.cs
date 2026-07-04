using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class ExamSessionAnswerModel : ExamSessionAnswerSaveModel
    {
        public List<long> SelectedAnswerOptionIds { get; set; }
        public string QuestionContent { get; set; }
        public string ExamSessionInfo { get; set; }
    }

    public class ExamSessionAnswerSaveModel
    {
        public long Id { get; set; }
        public long ExamSessionId { get; set; }
        public long QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public decimal Score { get; set; }
    }

    public class ExamSessionAnswerSearchModel : BaseSearch
    {
        public long? ExamSessionId { get; set; }
        public long? QuestionId { get; set; }
    }
}
