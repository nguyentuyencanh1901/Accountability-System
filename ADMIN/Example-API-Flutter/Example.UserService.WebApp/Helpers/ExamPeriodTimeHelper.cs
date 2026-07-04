using Example.Common.Enums;
using Example.Common.Utilities;
using Example.UserService.API.Models;

namespace Example.UserService.WebApp.Helpers
{
    /// <summary>Trạng thái thời gian kỳ thi trên UI WebApp (chưa đến giờ / đang mở / đã kết thúc).</summary>
    public enum ExamPeriodAvailability
    {
        NotYetStarted,
        Open,
        Ended
    }

    public static class ExamPeriodTimeHelper
    {
        public static ExamPeriodAvailability GetAvailability(ExamPeriodAssignmentModel assignment)
        {
            // API đã đóng kỳ → coi như kết thúc dù chưa hết giờ local
            if (assignment.ExamPeriodStatus == (int)ExamPeriodStatusEnum.Closed)
                return ExamPeriodAvailability.Ended;

            var now = VietnamTimeHelper.ToVietnamTime(DateTimeOffset.UtcNow);

            if (assignment.ExamPeriodEndAt.HasValue)
            {
                var end = VietnamTimeHelper.ToVietnamTime(assignment.ExamPeriodEndAt.Value);
                if (now > end)
                    return ExamPeriodAvailability.Ended;
            }

            if (assignment.ExamPeriodStartAt.HasValue)
            {
                var start = VietnamTimeHelper.ToVietnamTime(assignment.ExamPeriodStartAt.Value);
                if (now < start)
                    return ExamPeriodAvailability.NotYetStarted;
            }

            return ExamPeriodAvailability.Open;
        }

        public static bool CanStartExam(ExamPeriodAssignmentModel assignment)
        {
            if (assignment.Status == (int)ExamPeriodAssignmentStatusEnum.Completed
                || assignment.Status == (int)ExamPeriodAssignmentStatusEnum.Cancelled)
                return false;

            // Chỉ cho bấm "Bắt đầu" khi đang trong khung giờ kỳ thi
            return GetAvailability(assignment) == ExamPeriodAvailability.Open;
        }

        public static bool CanContinueExam(ExamPeriodAssignmentModel assignment)
        {
            if (assignment.Status == (int)ExamPeriodAssignmentStatusEnum.Completed
                || assignment.Status == (int)ExamPeriodAssignmentStatusEnum.Cancelled)
                return false;

            // Cho tiếp tục khi chưa hết kỳ (kể cả trước giờ mở — nếu đã có session InProgress từ trước)
            return GetAvailability(assignment) != ExamPeriodAvailability.Ended;
        }
    }
}
