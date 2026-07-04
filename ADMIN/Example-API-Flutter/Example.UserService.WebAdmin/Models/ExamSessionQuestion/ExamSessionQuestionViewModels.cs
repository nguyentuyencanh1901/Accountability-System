using Example.UserService.API.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.ExamSessionQuestion
{
    public class ExamSessionQuestionIndexViewModel : PagedViewModel
    {
        public List<ExamSessionQuestionModel> Items { get; set; } = new();
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

    public class ExamSessionQuestionFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bài thi")]
        [Display(Name = "Bài thi")]
        public long ExamSessionId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn câu hỏi")]
        [Display(Name = "Câu hỏi")]
        public long QuestionId { get; set; }

        [Display(Name = "Điểm")]
        public int Points { get; set; } = 1;

        [Display(Name = "Thứ tự")]
        public int SortOrder { get; set; }

        public List<SelectListItem> ExamSessionOptions { get; set; } = new();
        public List<SelectListItem> QuestionOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsEdit => Id > 0;
    }
}
