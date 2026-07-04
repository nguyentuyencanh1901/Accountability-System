using Example.UserService.API.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.ExamSessionAnswerOption
{
    public class ExamSessionAnswerOptionIndexViewModel : PagedViewModel
    {
        public List<ExamSessionAnswerOptionModel> Items { get; set; } = new();
        public long? ExamSessionAnswerIdFilter { get; set; }
        public long? AnswerOptionIdFilter { get; set; }
        public List<SelectListItem> ExamSessionAnswerFilterOptions { get; set; } = new();
        public List<SelectListItem> AnswerOptionFilterOptions { get; set; } = new();

        public override string BuildPageUrl(int pageIndex)
        {
            var parts = new List<string> { $"pageIndex={pageIndex}" };
            if (ExamSessionAnswerIdFilter.HasValue) parts.Add($"examSessionAnswerId={ExamSessionAnswerIdFilter}");
            if (AnswerOptionIdFilter.HasValue) parts.Add($"answerOptionId={AnswerOptionIdFilter}");
            return "?" + string.Join("&", parts);
        }
    }

    public class ExamSessionAnswerOptionFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn câu trả lời")]
        [Display(Name = "Câu trả lời")]
        public long ExamSessionAnswerId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đáp án")]
        [Display(Name = "Đáp án")]
        public long AnswerOptionId { get; set; }

        public List<SelectListItem> ExamSessionAnswerOptions { get; set; } = new();
        public List<SelectListItem> AnswerOptionOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsEdit => Id > 0;
    }
}
