using Example.UserService.API.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.ExamSessionAnswer
{
    public class ExamSessionAnswerIndexViewModel : PagedViewModel
    {
        public List<ExamSessionAnswerModel> Items { get; set; } = new();
        public long? ExamSessionIdFilter { get; set; }
        public long? QuestionIdFilter { get; set; }
        public List<SelectListItem> ExamSessionFilterOptions { get; set; } = new();
        public List<SelectListItem> QuestionFilterOptions { get; set; } = new();

        public override string BuildPageUrl(int pageIndex)
        {
            var parts = new List<string> { $"pageIndex={pageIndex}" };
            if (ExamSessionIdFilter.HasValue) parts.Add($"examSessionId={ExamSessionIdFilter}");
            if (QuestionIdFilter.HasValue) parts.Add($"questionId={QuestionIdFilter}");
            return "?" + string.Join("&", parts);
        }
    }

    public class ExamSessionAnswerFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bài thi")]
        [Display(Name = "Bài thi")]
        public long ExamSessionId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn câu hỏi")]
        [Display(Name = "Câu hỏi")]
        public long QuestionId { get; set; }

        [Display(Name = "Đúng")]
        public bool IsCorrect { get; set; }

        [Display(Name = "Điểm")]
        public decimal Score { get; set; }

        public List<SelectListItem> ExamSessionOptions { get; set; } = new();
        public List<SelectListItem> QuestionOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsEdit => Id > 0;
    }
}
