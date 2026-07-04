using Example.UserService.API.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.ExamSession
{
    public class ExamSessionIndexViewModel : PagedViewModel
    {
        public List<ExamSessionModel> Items { get; set; } = new();
    }

    public class ExamSessionDetailViewModel
    {
        public ExamSessionModel? Item { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ExamSessionFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bộ đề")]
        [Display(Name = "Bộ đề thi")]
        public long ExamSetId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thí sinh")]
        [Display(Name = "Thí sinh")]
        public long UserId { get; set; }

        [Display(Name = "Loại thi")]
        public int ExamType { get; set; }

        [Display(Name = "Bắt đầu")]
        public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.Now;

        [Display(Name = "Kết thúc")]
        public DateTimeOffset? FinishedAt { get; set; }

        [Display(Name = "Tổng điểm")]
        public decimal TotalScore { get; set; }

        [Display(Name = "Điểm tối đa")]
        public decimal MaxScore { get; set; }

        [Display(Name = "Trạng thái")]
        public int Status { get; set; }

        public List<SelectListItem> ExamSetOptions { get; set; } = new();
        public List<SelectListItem> UserOptions { get; set; } = new();
        public List<SelectListItem> ExamTypeOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsEdit => Id > 0;
    }
}
