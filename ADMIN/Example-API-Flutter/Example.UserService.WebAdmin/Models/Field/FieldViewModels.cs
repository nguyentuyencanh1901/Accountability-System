using Example.UserService.API.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.Field
{
    public class FieldIndexViewModel : PagedViewModel
    {
        public List<FieldModel> Items { get; set; } = new();
    }

    public class FieldFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên lĩnh vực")]
        [Display(Name = "Tên lĩnh vực")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public int Status { get; set; } = 1;

        public List<SelectListItem> StatusOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsEdit => Id > 0;
    }
}
