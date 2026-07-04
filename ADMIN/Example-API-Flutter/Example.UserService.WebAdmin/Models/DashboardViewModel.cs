namespace Example.UserService.WebAdmin.Models
{
    public class DashboardViewModel
    {
        public long TotalTestTakers { get; set; }
        public long TotalExamSets { get; set; }
        public long TotalExamSessions { get; set; }
        public long CompletedSessions { get; set; }
        public long InProgressSessions { get; set; }
        public long ExpiredSessions { get; set; }
        public double AverageScorePercent { get; set; }
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public List<YearlyTestTakerStatViewModel> YearlyStats { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class YearlyTestTakerStatViewModel
    {
        public int Year { get; set; }
        public int TestTakerCount { get; set; }
        public int ExamSessionCount { get; set; }
        public int CompletedCount { get; set; }
        public int InProgressCount { get; set; }
        public int ExpiredCount { get; set; }
        public double AverageScorePercent { get; set; }
        public double PassRatePercent { get; set; }
        public int MaxSessionCount { get; set; }
    }
}
