using Example.UserService.API.Models;

namespace Example.UserService.WebApp.Models.Home
{
    public class DashboardViewModel
    {
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public long TotalExamSets { get; set; }
        public long InProgressSessions { get; set; }
        public long CompletedSessions { get; set; }
        public List<ExamSessionModel> RecentInProgress { get; set; } = new();
        public List<ExamSessionModel> RecentCompleted { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}
