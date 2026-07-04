using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class ExamSessionAnswerOptionModel : ExamSessionAnswerOptionSaveModel
    {
        public string AnswerOptionContent { get; set; }
        public string ExamSessionAnswerInfo { get; set; }
    }

    public class ExamSessionAnswerOptionSaveModel
    {
        public long Id { get; set; }
        public long ExamSessionAnswerId { get; set; }
        public long AnswerOptionId { get; set; }
    }

    public class ExamSessionAnswerOptionSearchModel : BaseSearch
    {
        public long? ExamSessionAnswerId { get; set; }
        public long? AnswerOptionId { get; set; }
    }
}
