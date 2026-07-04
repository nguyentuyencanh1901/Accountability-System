using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class ExamSetModel : ExamSetSaveModel
    {
    }

    public class ExamSetSaveModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RequiredTotalPoints { get; set; }
        public int QuestionCount { get; set; }
        public int EasyCount { get; set; }
        public int MediumCount { get; set; }
        public int HardCount { get; set; }
        public int Type { get; set; }
        public int DurationMinutes { get; set; }
        public int Status { get; set; }
        public List<long> FieldIds { get; set; } = new();
    }

    public class ExamSetSearchModel : BaseSearch
    {
        public int? Type { get; set; }
    }
}
