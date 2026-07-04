using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class ExamSessionAnswerOption : EntityAuditBase<long>
    {
        public long ExamSessionAnswerId { get; set; }
        public ExamSessionAnswer ExamSessionAnswer { get; set; }

        public long AnswerOptionId { get; set; }
        public AnswerOption AnswerOption { get; set; }
    }
}
