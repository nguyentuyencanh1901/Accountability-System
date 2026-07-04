using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Enums
{
    /// <summary>Enum dùng chung toàn hệ thống thi trắc nghiệm.</summary>
    public enum StatusEnum
    {
        [Description("Hoạt động")]
        Active = 1,
        [Description("Ngừng hoạt động")]
        InActive = 2
    }

    public enum AIProcessEnum
    {
        [Description("Hoạt động")]
        Active = 1,
        [Description("Ngừng hoạt động")]
        InActive = 0
    }

    public enum ApproveStatusEnum
    {
        [Description("Tạo mới")]
        Create = 1,
        [Description("Công bố")]
        Publish = 2,
        [Description("Hủy")]
        Cancel = 3
    }

    public enum UserTypeEnum
    {
        [Description("Người quản lý")]
        Manager = 1,
        [Description("Người thi")]
        TestTaker = 2
    }

    // --- Module thi trắc nghiệm ---

    public enum QuestionTypeEnum
    {
        [Description("Chọn một đáp án")]
        SingleChoice = 1,
        [Description("Chọn nhiều đáp án")]
        MultipleChoice = 2
    }

    public enum DifficultyLevelEnum
    {
        [Description("Dễ")]
        Easy = 1,
        [Description("Trung bình")]
        Medium = 2,
        [Description("Khó")]
        Hard = 3
    }

    public enum ExamTypeEnum
    {
        [Description("Thi thử")]
        Trial = 1,
        [Description("Thi thật")]
        Real = 2
    }

    /// <summary>Loại bộ đề: 1=Thi thật, 2=Thi thử (theo nghiệp vụ bộ đề).</summary>
    public enum ExamSetTypeEnum
    {
        [Description("Thi thật")]
        Real = 1,
        [Description("Thi thử")]
        Trial = 2
    }

    public enum ExamSessionStatusEnum
    {
        [Description("Đang thi")]
        InProgress = 1,
        [Description("Hoàn thành")]
        Completed = 2,
        [Description("Hết hạn")]
        Expired = 3,
        [Description("Bị hủy bỏ")]
        Cancelled = 4
    }

    public enum ExamPeriodStatusEnum
    {
        [Description("Nháp")]
        Draft = 1,
        [Description("Đã công bố")]
        Published = 2,
        [Description("Đã đóng")]
        Closed = 3
    }

    public enum ExamPeriodAssignmentStatusEnum
    {
        [Description("Đã phân công")]
        Assigned = 1,
        [Description("Đang thi")]
        InProgress = 2,
        [Description("Hoàn thành")]
        Completed = 3,
        [Description("Vắng thi")]
        Absent = 4,
        [Description("Bị hủy bỏ")]
        Cancelled = 5
    }
}
