using Example.UserService.API.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.AnswerOption
{
    public class AnswerOptionIndexViewModel : PagedViewModel
    {
        public long QuestionId { get; set; }
        public long FieldId { get; set; }
        public string? FieldName { get; set; }
        public string? QuestionContent { get; set; }
        public List<AnswerOptionModel> Items { get; set; } = new();

        public override string BuildPageUrl(int pageIndex)
            => $"?pageIndex={pageIndex}&questionId={QuestionId}";
    }

    public class AnswerOptionFormViewModel
    {
        public long Id { get; set; }

        public long FieldId { get; set; }
        public string? FieldName { get; set; }
        public string? QuestionContent { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn câu hỏi")]
        [Display(Name = "Câu hỏi")]
        public long QuestionId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đáp án")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Đáp án đúng")]
        public bool IsCorrect { get; set; }

        [Display(Name = "Thứ tự")]
        public int SortOrder { get; set; }

        public List<SelectListItem> QuestionOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsEdit => Id > 0;
        public bool IsQuestionFixed => QuestionId > 0;
    }
}
