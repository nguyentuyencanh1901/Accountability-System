using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.ExamPeriod
{
    public class ExamPeriodIndexViewModel : PagedViewModel
    {
        public List<ExamPeriodModel> Items { get; set; } = new();
    }

    public class ExamPeriodFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên kỳ thi")]
        [Display(Name = "Tên kỳ thi")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        [Display(Name = "Thời gian bắt đầu")]
        public DateTimeOffset StartAt { get; set; } = DateTimeOffset.Now;

        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        [Display(Name = "Thời gian kết thúc")]
        public DateTimeOffset EndAt { get; set; } = DateTimeOffset.Now.AddDays(7);

        /// <summary>Giá trị StartAt ban đầu khi sửa — cho phép giữ nguyên nếu kỳ thi đã bắt đầu.</summary>
        public DateTimeOffset? OriginalStartAt { get; set; }

        [Display(Name = "Trạng thái")]
        public int Status { get; set; } = 1;

        [Display(Name = "Loại bộ đề")]
        public int ExamSetTypeFilter { get; set; } = (int)Example.Common.Enums.ExamSetTypeEnum.Real;

        [Display(Name = "Thí sinh")]
        public List<long> UserIds { get; set; } = new();

        [Display(Name = "Bộ đề")]
        public List<long> ExamSetIds { get; set; } = new();

        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<SelectListItem> ExamSetTypeOptions { get; set; } = new();
        public List<SelectListItem> UserOptions { get; set; } = new();
        public List<ExamSetPickOptionViewModel> ExamSetOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsEdit => Id > 0;
    }

    public class ExamPeriodDetailViewModel
    {
        public ExamPeriodModel? Period { get; set; }
        public int AssignedCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int AbsentCount { get; set; }
        public List<ExamPeriodAssignmentModel> Assignments { get; set; } = new();
        public int? FilterStatus { get; set; }
        public int? FilterExamType { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
