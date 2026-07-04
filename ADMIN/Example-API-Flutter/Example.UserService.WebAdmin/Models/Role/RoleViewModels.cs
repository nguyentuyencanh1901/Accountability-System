using Example.UserService.API.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.Role
{
    public class RoleIndexViewModel : PagedViewModel
    {
        public List<RoleModel> Items { get; set; } = new();
    }

    public class RoleFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên vai trò")]
        [Display(Name = "Tên vai trò")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Quyền hạn")]
        public List<long> Permissions { get; set; } = new();

        public List<SelectListItem> PermissionOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsEdit => Id > 0;
    }
}
