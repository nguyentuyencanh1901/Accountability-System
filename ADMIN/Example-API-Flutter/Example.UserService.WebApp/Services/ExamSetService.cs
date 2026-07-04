using Example.Common.Enums;
using Example.Common.Utilities;
using Example.UserService.API.Models;
using Example.UserService.WebApp.Helpers;
using Example.UserService.WebApp.Models.ExamSet;
using Example.UserService.WebApp.Repository.IRepository;
using Example.UserService.WebApp.Services.IServices;

namespace Example.UserService.WebApp.Services
{
    /// <summary>
    /// Dịch vụ danh sách kỳ thi được phân công: gộp dữ liệu phân công + bộ đề + session đang làm,
    /// và quyết định hiển thị nút bắt đầu/tiếp tục theo khung giờ kỳ thi và trạng thái phân công.
    /// </summary>
    public class ExamSetService : IExamSetService
    {
        private readonly IExamSetRepository _examSetRepository;
        private readonly IExamSessionRepository _examSessionRepository;
        private readonly IExamPeriodAssignmentRepository _assignmentRepository;

        // Tiêm ba repository: phân công, bộ đề và phiên thi để gộp dữ liệu trang index/chi tiết.
        public ExamSetService(
            IExamSetRepository examSetRepository,
            IExamSessionRepository examSessionRepository,
            IExamPeriodAssignmentRepository assignmentRepository)
        {
            _examSetRepository = examSetRepository;
            _examSessionRepository = examSessionRepository;
            _assignmentRepository = assignmentRepository;
        }

        /// <summary>Trang index: liệt kê mọi phân công của thí sinh kèm thông tin bộ đề (nếu đã gán).</summary>
        public async Task<ExamSetIndexViewModel> GetIndexAsync()
        {
            var assignmentsResult = await _assignmentRepository.GetMyAssignmentsAsync();
            if (!assignmentsResult.Success)
            {
                return new ExamSetIndexViewModel { ErrorMessage = assignmentsResult.Message };
            }

            var assignments = assignmentsResult.Data ?? new List<ExamPeriodAssignmentModel>();
            var items = new List<AssignedExamItemViewModel>();

            foreach (var assignment in assignments)
            {
                // Bộ đề có thể null (random khi start) — chỉ load nếu đã gán sẵn ExamSetId.
                ExamSetModel? examSet = null;
                if (assignment.ExamSetId > 0)
                {
                    var examSetResult = await _examSetRepository.GetByIdAsync(assignment.ExamSetId.Value);
                    // Quy tắc nghiệp vụ: chỉ hiển thị bộ đề đang Active trên danh sách.
                    if (examSetResult.Success && examSetResult.Data != null
                        && examSetResult.Data.Status == (int)StatusEnum.Active)
                    {
                        examSet = examSetResult.Data;
                    }
                }

                items.Add(new AssignedExamItemViewModel
                {
                    Assignment = assignment,
                    ExamSet = examSet
                });
            }

            return new ExamSetIndexViewModel { Items = items };
        }

        /// <summary>
        /// Trang chi tiết kỳ thi: xác minh phân công thuộc user, load bộ đề và session InProgress phù hợp.
        /// </summary>
        public async Task<(ExamSetDetailsViewModel? Model, string? ErrorMessage)> GetDetailsAsync(long assignmentId, long userId)
        {
            var assignmentsResult = await _assignmentRepository.GetMyAssignmentsAsync();
            if (!assignmentsResult.Success)
                return (null, assignmentsResult.Message ?? "Không thể tải phân công.");

            // Quy tắc nghiệp vụ: chỉ hiển thị phân công nằm trong danh sách của user hiện tại (chống truy cập chéo).
            var assignment = (assignmentsResult.Data ?? new List<ExamPeriodAssignmentModel>())
                .FirstOrDefault(x => x.Id == assignmentId);

            if (assignment == null)
                return (null, "Bạn chưa được phân công kỳ thi này.");

            ExamSetModel? examSet = null;
            if (assignment.ExamSetId > 0)
            {
                var examSetResult = await _examSetRepository.GetByIdAsync(assignment.ExamSetId.Value);
                if (examSetResult.Success && examSetResult.Data != null)
                    examSet = examSetResult.Data;
            }

            // Tìm session InProgress cùng loại thi (và cùng bộ đề nếu phân công đã gán sẵn).
            var inProgressResult = await _examSessionRepository.GetListAsync(new ExamSessionSearchModel
            {
                PageIndex = 1,
                PageSize = 10,
                UserId = userId,
                Status = (int)ExamSessionStatusEnum.InProgress
            });
            var inProgress = new List<ExamSessionModel>();
            // Quy tắc nghiệp vụ: chỉ cho tiếp tục khi phân công chưa hoàn thành và còn trong khung giờ thi.
            if (assignment.Status != (int)ExamPeriodAssignmentStatusEnum.Completed
                && assignment.Status != (int)ExamPeriodAssignmentStatusEnum.Cancelled
                && ExamPeriodTimeHelper.CanContinueExam(assignment))
            {
                inProgress = (inProgressResult.Data ?? new List<ExamSessionModel>())
                    .Where(s => s.ExamType == assignment.ExamType
                        && (assignment.ExamSetId == null || assignment.ExamSetId <= 0 || s.ExamSetId == assignment.ExamSetId))
                    .ToList();
            }

            return (new ExamSetDetailsViewModel
            {
                Assignment = assignment,
                ExamSet = examSet,
                InProgressSessions = inProgress,
                CanStartExam = CanStartExam(assignment),
                CanContinueExam = CanContinueExam(assignment),
                PeriodStatusName = GetExamPeriodStatusName(assignment),
                DisplayStatusName = GetDisplayStatusName(assignment)
            }, null);
        }

        /// <summary>Ánh xạ trạng thái khung giờ kỳ thi sang nhãn hiển thị cho thí sinh.</summary>
        public string GetExamPeriodStatusName(ExamPeriodAssignmentModel assignment)
        {
            var availability = ExamPeriodTimeHelper.GetAvailability(assignment);
            return availability switch
            {
                ExamPeriodAvailability.NotYetStarted => "Đã công bố",
                ExamPeriodAvailability.Open => "Đã công bố",
                ExamPeriodAvailability.Ended => "Đã đóng",
                _ => DisplayHelper.EnumName<ExamPeriodStatusEnum>(assignment.ExamPeriodStatus)
            };
        }

        /// <summary>Nhãn trạng thái tổng hợp: ưu tiên hết giờ/chưa đến giờ trước trạng thái phân công.</summary>
        public string GetDisplayStatusName(ExamPeriodAssignmentModel assignment)
        {
            var availability = ExamPeriodTimeHelper.GetAvailability(assignment);
            if (availability == ExamPeriodAvailability.Ended)
                return "Kết thúc";
            if (availability == ExamPeriodAvailability.NotYetStarted)
                return "Chưa đến giờ thi";
            return GetAssignmentStatusName(assignment.Status);
        }

        /// <summary>Quy tắc: được bắt đầu thi khi khung giờ mở và phân công cho phép.</summary>
        public bool CanStartExam(ExamPeriodAssignmentModel assignment)
            => ExamPeriodTimeHelper.CanStartExam(assignment);

        /// <summary>Quy tắc: được tiếp tục khi còn trong khung giờ và chưa hoàn thành phân công.</summary>
        public bool CanContinueExam(ExamPeriodAssignmentModel assignment)
            => ExamPeriodTimeHelper.CanContinueExam(assignment);

        /// <summary>Chuyển mã loại thi (Trial/Real) sang tên tiếng Việt.</summary>
        public string GetExamTypeName(int examType) => examType switch
        {
            (int)ExamTypeEnum.Trial => "Thi thử",
            (int)ExamTypeEnum.Real => "Thi thật",
            _ => DisplayHelper.EnumName<ExamTypeEnum>(examType)
        };

        /// <summary>Chuyển mã trạng thái phân công sang nhãn hiển thị.</summary>
        public string GetAssignmentStatusName(int status) => status switch
        {
            (int)ExamPeriodAssignmentStatusEnum.Assigned => "Đã phân công",
            (int)ExamPeriodAssignmentStatusEnum.InProgress => "Đang thi",
            (int)ExamPeriodAssignmentStatusEnum.Completed => "Hoàn thành",
            (int)ExamPeriodAssignmentStatusEnum.Absent => "Vắng thi",
            (int)ExamPeriodAssignmentStatusEnum.Cancelled => "Bị hủy bỏ",
            _ => DisplayHelper.EnumName<ExamPeriodAssignmentStatusEnum>(status)
        };
    }
}
