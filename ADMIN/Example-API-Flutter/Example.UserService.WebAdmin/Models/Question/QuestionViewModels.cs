using Example.UserService.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.Question
{
    public class QuestionIndexViewModel : PagedViewModel
    {
        public long? FieldIdFilter { get; set; }
        public int? QuestionTypeFilter { get; set; }
        public int? DifficultyLevelFilter { get; set; }
        public string? Keyword { get; set; }

        public List<QuestionModel> Items { get; set; } = new();
        public string? ApiBaseUrl { get; set; }

        public List<SelectListItem> FieldFilterOptions { get; set; } = new();
        public List<SelectListItem> QuestionTypeFilterOptions { get; set; } = new();
        public List<SelectListItem> DifficultyFilterOptions { get; set; } = new();

        public override string BuildPageUrl(int pageIndex)
        {
            var query = $"?pageIndex={pageIndex}";
            if (FieldIdFilter > 0) query += $"&fieldId={FieldIdFilter}";
            if (QuestionTypeFilter.HasValue) query += $"&questionType={QuestionTypeFilter}";
            if (DifficultyLevelFilter.HasValue) query += $"&difficultyLevel={DifficultyLevelFilter}";
            if (!string.IsNullOrWhiteSpace(Keyword)) query += $"&keyword={Uri.EscapeDataString(Keyword)}";
            return query;
        }
    }

    public class AnswerOptionFormViewModel
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int SortOrder { get; set; }
    }

    public class QuestionFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lĩnh vực")]
        [Display(Name = "Lĩnh vực")]
        public long FieldId { get; set; }

        public string? ImageUrl { get; set; }
        public string? ImagePreviewUrl { get; set; }
        public bool RemoveImage { get; set; }

        [Display(Name = "Ảnh minh họa")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Điểm")]
        public int Points { get; set; } = 1;

        [Display(Name = "Loại câu hỏi")]
        public int QuestionType { get; set; } = 1;

        [Display(Name = "Độ khó")]
        public int DifficultyLevel { get; set; } = 2;

        [Display(Name = "Trạng thái")]
        public int Status { get; set; } = 1;

        public List<AnswerOptionFormViewModel> AnswerOptions { get; set; } = new()
        {
            new() { SortOrder = 1 },
            new() { SortOrder = 2 }
        };

        public List<SelectListItem> QuestionTypeOptions { get; set; } = new();
        public List<SelectListItem> FieldOptions { get; set; } = new();
        public List<SelectListItem> DifficultyLevelOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsEdit => Id > 0;
    }
}
