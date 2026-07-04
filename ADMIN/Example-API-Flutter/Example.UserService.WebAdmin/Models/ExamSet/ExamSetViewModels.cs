using Example.UserService.API.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.ExamSet
{
    public class ExamSetIndexViewModel : PagedViewModel
    {
        public List<ExamSetModel> Items { get; set; } = new();
    }

    public class ExamSetTakersViewModel : PagedViewModel
    {
        public long ExamSetId { get; set; }
        public string ExamSetName { get; set; } = string.Empty;
        public List<ExamSetTakerModel> Items { get; set; } = new();

        public override string BuildPageUrl(int pageIndex) => $"/ExamSet/Takers/{ExamSetId}?pageIndex={pageIndex}";
    }

    public class ExamSetFieldOptionViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }

    public class ExamSetFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bộ đề")]
        [Display(Name = "Tên bộ đề")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Loại đề")]
        public int Type { get; set; } = (int)Example.Common.Enums.ExamSetTypeEnum.Real;

        [Display(Name = "Tổng điểm yêu cầu")]
        public int RequiredTotalPoints { get; set; } = 10;

        [Display(Name = "Số câu hỏi")]
        public int QuestionCount { get; set; } = 10;

        [Display(Name = "Số câu Dễ")]
        public int EasyCount { get; set; }

        [Display(Name = "Số câu Trung bình")]
        public int MediumCount { get; set; }

        [Display(Name = "Số câu Khó")]
        public int HardCount { get; set; }

        [Display(Name = "Thời gian (phút)")]
        public int DurationMinutes { get; set; } = 60;

        [Display(Name = "Trạng thái")]
        public int Status { get; set; } = 1;

        public List<long> FieldIds { get; set; } = new();
        public List<ExamSetFieldOptionViewModel> FieldOptions { get; set; } = new();
        public List<SelectListItem> TypeOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsEdit => Id > 0;
    }
}
