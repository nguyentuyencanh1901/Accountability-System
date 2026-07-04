using Example.Common.Enums;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade trang chủ/dashboard: tổng hợp số liệu từ API bộ đề và phiên thi,
    /// tính thống kê thí sinh theo năm phía WebAdmin (không dùng ApiListHelper vì cần MetaData/aggregate).
    /// </summary>
    public class HomeService : IHomeService
    {
        // Ngưỡng điểm đạt (%) dùng tính tỷ lệ đỗ trên dashboard
        private const double PassThresholdPercent = 50;

        // Repository bộ đề — đếm tổng số bộ đề từ MetaData
        private readonly IExamSetRepository _examSetRepository;
        // Repository phiên thi — tải danh sách để thống kê thí sinh theo năm
        private readonly IExamSessionRepository _examSessionRepository;

        /// <summary>Khởi tạo service với repository bộ đề và phiên thi.</summary>
        public HomeService(IExamSetRepository examSetRepository, IExamSessionRepository examSessionRepository)
        {
            _examSetRepository = examSetRepository;
            _examSessionRepository = examSessionRepository;
        }

        /// <summary>
        /// Dựng ViewModel dashboard: đếm bộ đề từ MetaData; tải phiên thi để đếm thí sinh và group theo năm StartedAt.
        /// </summary>
        public async Task<DashboardViewModel> BuildDashboardAsync(string? fullName, string? username)
        {
            var vm = new DashboardViewModel
            {
                FullName = fullName,
                Username = username
            };

            var examSetResult = await _examSetRepository.GetListAsync(new ExamSetSearchModel { PageIndex = 1, PageSize = 1 });
            // PageSize=1 chỉ cần MetaData.TotalItems, không cần tải toàn bộ danh sách
            if (examSetResult.Success)
                vm.TotalExamSets = examSetResult.MetaData?.TotalItems ?? examSetResult.Data?.Count ?? 0;
            else
                vm.ErrorMessage = examSetResult.Message;

            var sessionResult = await _examSessionRepository.GetListAsync(new ExamSessionSearchModel { PageIndex = 1, PageSize = 5000 });
            if (sessionResult.Success && sessionResult.Data != null)
            {
                var sessions = sessionResult.Data;
                vm.TotalExamSessions = sessionResult.MetaData?.TotalItems ?? sessions.Count;
                vm.TotalTestTakers = sessions.Select(s => s.UserId).Distinct().Count();
                vm.CompletedSessions = sessions.Count(s => s.Status == (int)ExamSessionStatusEnum.Completed);
                vm.InProgressSessions = sessions.Count(s => s.Status == (int)ExamSessionStatusEnum.InProgress);
                vm.ExpiredSessions = sessions.Count(s => s.Status == (int)ExamSessionStatusEnum.Expired);
                vm.AverageScorePercent = CalcAverageScorePercent(sessions);

                vm.YearlyStats = sessions
                    .GroupBy(s => s.StartedAt.Year)
                    .Select(g =>
                    {
                        var list = g.ToList();
                        var finished = list.Where(IsFinished).ToList();
                        var passCount = finished.Count(s => s.MaxScore > 0 && ScorePercent(s) >= PassThresholdPercent);
                        return new YearlyTestTakerStatViewModel
                        {
                            Year = g.Key,
                            TestTakerCount = list.Select(x => x.UserId).Distinct().Count(),
                            ExamSessionCount = list.Count,
                            CompletedCount = list.Count(x => x.Status == (int)ExamSessionStatusEnum.Completed),
                            InProgressCount = list.Count(x => x.Status == (int)ExamSessionStatusEnum.InProgress),
                            ExpiredCount = list.Count(x => x.Status == (int)ExamSessionStatusEnum.Expired),
                            AverageScorePercent = CalcAverageScorePercent(list),
                            PassRatePercent = finished.Count > 0
                                ? Math.Round((double)passCount / finished.Count * 100, 1)
                                : 0
                        };
                    })
                    .OrderByDescending(x => x.Year)
                    .ToList();

                if (vm.YearlyStats.Count > 0)
                {
                    // Chuẩn hóa chiều cao biểu đồ: gán max cho mỗi năm
                    var maxCount = vm.YearlyStats.Max(x => x.ExamSessionCount);
                    foreach (var stat in vm.YearlyStats)
                        stat.MaxSessionCount = maxCount;
                }
            }
            else if (string.IsNullOrWhiteSpace(vm.ErrorMessage))
            {
                vm.ErrorMessage = sessionResult.Message;
            }

            return vm;
        }

        /// <summary>Phiên thi đã kết thúc (hoàn thành hoặc hết giờ) — dùng tính điểm trung bình/tỷ lệ đỗ.</summary>
        private static bool IsFinished(ExamSessionModel s) =>
            s.Status == (int)ExamSessionStatusEnum.Completed
            || s.Status == (int)ExamSessionStatusEnum.Expired;

        private static double ScorePercent(ExamSessionModel s) =>
            s.MaxScore > 0 ? (double)s.TotalScore / (double)s.MaxScore * 100 : 0;

        /// <summary>Tính điểm trung bình (%) trên các phiên đã kết thúc có MaxScore &gt; 0.</summary>
        private static double CalcAverageScorePercent(IEnumerable<ExamSessionModel> sessions)
        {
            var scored = sessions.Where(s => IsFinished(s) && s.MaxScore > 0).ToList();
            if (scored.Count == 0)
                return 0;
            return Math.Round(scored.Average(ScorePercent), 1);
        }
    }
}
