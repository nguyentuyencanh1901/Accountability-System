using Example.Common.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Example.UserService.WebAdmin.Helpers
{
    public static class SelectListHelper
    {
        public static List<SelectListItem> UserTypes(int selected = 2) => new()
        {
            new("Người quản lý", ((int)UserTypeEnum.Manager).ToString(), selected == (int)UserTypeEnum.Manager),
            new("Người thi", ((int)UserTypeEnum.TestTaker).ToString(), selected == (int)UserTypeEnum.TestTaker)
        };

        public static List<SelectListItem> Statuses(int selected = 1) => new()
        {
            new("Hoạt động", ((int)StatusEnum.Active).ToString(), selected == (int)StatusEnum.Active),
            new("Ngừng hoạt động", ((int)StatusEnum.InActive).ToString(), selected == (int)StatusEnum.InActive)
        };

        public static List<SelectListItem> QuestionTypes(int selected = 1) => new()
        {
            new("Chọn một đáp án", ((int)QuestionTypeEnum.SingleChoice).ToString(), selected == (int)QuestionTypeEnum.SingleChoice),
            new("Chọn nhiều đáp án", ((int)QuestionTypeEnum.MultipleChoice).ToString(), selected == (int)QuestionTypeEnum.MultipleChoice)
        };

        public static List<SelectListItem> DifficultyLevels(int selected = (int)DifficultyLevelEnum.Medium) => new()
        {
            new("Dễ", ((int)DifficultyLevelEnum.Easy).ToString(), selected == (int)DifficultyLevelEnum.Easy),
            new("Trung bình", ((int)DifficultyLevelEnum.Medium).ToString(), selected == (int)DifficultyLevelEnum.Medium),
            new("Khó", ((int)DifficultyLevelEnum.Hard).ToString(), selected == (int)DifficultyLevelEnum.Hard)
        };

        public static List<SelectListItem> AllOption(string label, string value = "") => new()
        {
            new(label, value, string.IsNullOrEmpty(value))
        };

        public static List<SelectListItem> ExamSetTypes(int selected = (int)ExamSetTypeEnum.Real) => new()
        {
            new("Thi thật", ((int)ExamSetTypeEnum.Real).ToString(), selected == (int)ExamSetTypeEnum.Real),
            new("Thi thử", ((int)ExamSetTypeEnum.Trial).ToString(), selected == (int)ExamSetTypeEnum.Trial)
        };

        public static List<SelectListItem> ExamTypes(int selected = 0) => new()
        {
            new("Thi thử", ((int)ExamTypeEnum.Trial).ToString(), selected == (int)ExamTypeEnum.Trial),
            new("Thi thật", ((int)ExamTypeEnum.Real).ToString(), selected == (int)ExamTypeEnum.Real)
        };

        public static List<SelectListItem> ExamSessionStatuses(int selected = 0) => new()
        {
            new("Đang thi", ((int)ExamSessionStatusEnum.InProgress).ToString(), selected == (int)ExamSessionStatusEnum.InProgress),
            new("Hoàn thành", ((int)ExamSessionStatusEnum.Completed).ToString(), selected == (int)ExamSessionStatusEnum.Completed),
            new("Hết hạn", ((int)ExamSessionStatusEnum.Expired).ToString(), selected == (int)ExamSessionStatusEnum.Expired),
            new("Bị hủy bỏ", ((int)ExamSessionStatusEnum.Cancelled).ToString(), selected == (int)ExamSessionStatusEnum.Cancelled)
        };

        public static List<SelectListItem> ExamPeriodStatuses(int selected = 1) => new()
        {
            new("Nháp", ((int)ExamPeriodStatusEnum.Draft).ToString(), selected == (int)ExamPeriodStatusEnum.Draft),
            new("Đã công bố", ((int)ExamPeriodStatusEnum.Published).ToString(), selected == (int)ExamPeriodStatusEnum.Published),
            new("Đã đóng", ((int)ExamPeriodStatusEnum.Closed).ToString(), selected == (int)ExamPeriodStatusEnum.Closed)
        };

        public static List<SelectListItem> ExamPeriodAssignmentStatuses(int selected = 1) => new()
        {
            new("Đã phân công", ((int)ExamPeriodAssignmentStatusEnum.Assigned).ToString(), selected == (int)ExamPeriodAssignmentStatusEnum.Assigned),
            new("Đang thi", ((int)ExamPeriodAssignmentStatusEnum.InProgress).ToString(), selected == (int)ExamPeriodAssignmentStatusEnum.InProgress),
            new("Hoàn thành", ((int)ExamPeriodAssignmentStatusEnum.Completed).ToString(), selected == (int)ExamPeriodAssignmentStatusEnum.Completed),
            new("Vắng thi", ((int)ExamPeriodAssignmentStatusEnum.Absent).ToString(), selected == (int)ExamPeriodAssignmentStatusEnum.Absent),
            new("Bị hủy bỏ", ((int)ExamPeriodAssignmentStatusEnum.Cancelled).ToString(), selected == (int)ExamPeriodAssignmentStatusEnum.Cancelled)
        };

    }
}
