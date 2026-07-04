using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.ExamPeriodAssignment
{
    public class ExamPeriodAssignmentIndexViewModel : PagedViewModel
    {
        public long? ExamPeriodId { get; set; }
        public string? ExamPeriodName { get; set; }
        public List<ExamPeriodAssignmentModel> Items { get; set; } = new();

        public override string BuildPageUrl(int pageIndex)
            => ExamPeriodId > 0
                ? $"?examPeriodId={ExamPeriodId}&pageIndex={pageIndex}"
                : $"?pageIndex={pageIndex}";
    }

    public class ExamPeriodAssignmentFormViewModel
    {
        public long Id { get; set; }

        [Display(Name = "Kỳ thi")]
        [Range(1, long.MaxValue, ErrorMessage = "Vui lòng chọn kỳ thi")]
        public long ExamPeriodId { get; set; }

        [Display(Name = "Thí sinh")]
        [Range(1, long.MaxValue, ErrorMessage = "Vui lòng chọn thí sinh")]
        public long UserId { get; set; }

        [Display(Name = "Bộ đề")]
        public long ExamSetId { get; set; }

        public List<long> ExamSetIds { get; set; } = new();

        [Display(Name = "Loại thi")]
        public int ExamType { get; set; } = 2;

        [Display(Name = "Trạng thái")]
        public int Status { get; set; } = 1;

        public long? ExamSessionId { get; set; }

        public List<SelectListItem> ExamPeriodOptions { get; set; } = new();
        public List<SelectListItem> UserOptions { get; set; } = new();
        public List<SelectListItem> ExamSetOptions { get; set; } = new();
        public List<ExamSetPickOptionViewModel> ExamSetPickOptions { get; set; } = new();
        public List<SelectListItem> ExamTypeOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsEdit => Id > 0;
    }
}
