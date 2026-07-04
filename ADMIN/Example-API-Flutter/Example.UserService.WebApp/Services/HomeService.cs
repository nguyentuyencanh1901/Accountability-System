using Example.Common.Enums;
using Example.UserService.API.Models;
using Example.UserService.WebApp.Models.Home;
using Example.UserService.WebApp.Repository.IRepository;
using Example.UserService.WebApp.Services.IServices;

namespace Example.UserService.WebApp.Services
{
    /// <summary>
    /// Dịch vụ trang chủ (dashboard): tổng hợp số liệu phân công, phiên đang thi và lịch sử gần đây
    /// từ nhiều Repository, áp dụng quy tắc lọc để ẩn session thuộc phân công đã hoàn thành.
    /// </summary>
    public class HomeService : IHomeService
    {
        private readonly IExamPeriodAssignmentRepository _assignmentRepository;
        private readonly IExamSessionRepository _examSessionRepository;

        // Tiêm repository phân công và phiên thi để tổng hợp số liệu dashboard.
        public HomeService(
            IExamPeriodAssignmentRepository assignmentRepository,
            IExamSessionRepository examSessionRepository)
        {
            _assignmentRepository = assignmentRepository;
            _examSessionRepository = examSessionRepository;
        }

        /// <summary>
        /// Dựng ViewModel dashboard: đếm phân công, lọc session InProgress hợp lệ, và lấy lịch sử gần nhất.
        /// </summary>
        public async Task<DashboardViewModel> BuildDashboardAsync(long userId, string? fullName, string? username)
        {
            var vm = new DashboardViewModel
            {
                FullName = fullName,
                Username = username
            };

            // --- Phân công kỳ thi: tổng số và danh sách Id đã hoàn thành ---
            var assignmentResult = await _assignmentRepository.GetMyAssignmentsAsync();
            var completedAssignmentIds = new HashSet<long>();
            if (assignmentResult.Success)
            {
                vm.TotalExamSets = assignmentResult.Data?.Count ?? 0;
                completedAssignmentIds = (assignmentResult.Data ?? new List<ExamPeriodAssignmentModel>())
                    .Where(x => x.Status == (int)ExamPeriodAssignmentStatusEnum.Completed)
                    .Select(x => x.Id)
                    .ToHashSet();
            }
            else if (string.IsNullOrWhiteSpace(vm.ErrorMessage))
                vm.ErrorMessage = assignmentResult.Message;

            // --- Phiên thi đang làm: tối đa 5 bản ghi, loại session gắn phân công đã Completed ---
            var inProgressResult = await _examSessionRepository.GetListAsync(new ExamSessionSearchModel
            {
                PageIndex = 1,
                PageSize = 5,
                UserId = userId,
                Status = (int)ExamSessionStatusEnum.InProgress
            });
            if (inProgressResult.Success)
            {
                var raw = inProgressResult.Data ?? new List<ExamSessionModel>();
                // Quy tắc nghiệp vụ: không hiển thị bài đang làm nếu phân công tương ứng đã đánh dấu hoàn thành.
                var filtered = raw
                    .Where(s => !s.ExamPeriodAssignmentId.HasValue
                                || !completedAssignmentIds.Contains(s.ExamPeriodAssignmentId.Value))
                    .ToList();

                vm.InProgressSessions = filtered.Count;
                vm.RecentInProgress = filtered;
            }

            // --- Lịch sử thi: 3 phiên gần nhất và tổng số từ metadata phân trang API ---
            var historyResult = await _examSessionRepository.GetHistoryAsync(new ExamSessionSearchModel
            {
                PageIndex = 1,
                PageSize = 3,
                UserId = userId
            });
            if (historyResult.Success)
            {
                vm.CompletedSessions = historyResult.MetaData?.TotalItems ?? historyResult.Data?.Count ?? 0;
                vm.RecentCompleted = historyResult.Data ?? new List<ExamSessionModel>();
            }

            return vm;
        }
    }
}
