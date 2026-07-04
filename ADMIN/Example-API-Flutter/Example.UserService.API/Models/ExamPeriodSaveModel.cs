using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class ExamPeriodModel : ExamPeriodSaveModel
    {
        public long AssignmentCount { get; set; }
    }

    public class ExamPeriodSaveModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public int Status { get; set; }               // 1=Nháp, 2=Đã công bố, 3=Đã đóng
        public List<long> UserIds { get; set; } = new();
        public List<long> ExamSetIds { get; set; } = new();
    }

    public class ExamPeriodSearchModel : BaseSearch
    {
    }

    public class ExamPeriodMonitoringModel
    {
        public ExamPeriodModel Period { get; set; }
        public int AssignedCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int AbsentCount { get; set; }
        public List<ExamPeriodAssignmentModel> Assignments { get; set; } = new();
    }
}
