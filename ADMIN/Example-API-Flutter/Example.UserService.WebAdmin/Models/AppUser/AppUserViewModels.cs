using Example.UserService.API.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Example.UserService.WebAdmin.Models.AppUser
{
    public class AppUserIndexViewModel : PagedViewModel
    {
        public List<AppUserModel> Items { get; set; } = new();
        public int UserTypeFilter { get; set; } = (int)Example.Common.Enums.UserTypeEnum.Manager;

        public override string BuildPageUrl(int pageIndex)
            => $"?pageIndex={pageIndex}&userType={UserTypeFilter}";
    }

    public class AppUserFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public int Status { get; set; } = 1;

        [Display(Name = "Loại tài khoản")]
        public int UserType { get; set; } = 2;

        [Display(Name = "Vai trò")]
        public List<long> Roles { get; set; } = new();

        public List<SelectListItem> RoleOptions { get; set; } = new();
        public List<SelectListItem> UserTypeOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsEdit => Id > 0;
        /// <summary>Sửa tài khoản SUPER_ADMIN — chỉ cho phép đổi thông tin cá nhân, không đổi quyền/loại tài khoản.</summary>
        public bool IsSuperAdminRestrictedEdit => IsEdit && IsSuperAdmin;
    }

    public class ResetPasswordViewModel
    {
        public long Id { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự")]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Xác nhận mật khẩu không khớp")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }
}
