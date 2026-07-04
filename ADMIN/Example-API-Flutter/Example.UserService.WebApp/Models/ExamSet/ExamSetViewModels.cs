using Example.UserService.API.Models;

namespace Example.UserService.WebApp.Models.ExamSet
{
    public class AssignedExamItemViewModel
    {
        public ExamPeriodAssignmentModel Assignment { get; set; } = new();
        public ExamSetModel? ExamSet { get; set; }
    }

    public class ExamSetIndexViewModel
    {
        public List<AssignedExamItemViewModel> Items { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class ExamSetDetailsViewModel
    {
        public ExamPeriodAssignmentModel Assignment { get; set; } = new();
        public ExamSetModel? ExamSet { get; set; }
        public List<ExamSessionModel> InProgressSessions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool CanStartExam { get; set; }
        public bool CanContinueExam { get; set; }
        public string PeriodStatusName { get; set; } = string.Empty;
        public string DisplayStatusName { get; set; } = string.Empty;
    }
}
