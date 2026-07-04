using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class AppUserModel : AppUserSaveModel
    {
    }
    public class AppUserSaveModel
    {
        public long Id { get; set; }
        public string Username { get; set; }      // Tên đăng nhập (unique)
        public string PasswordHash { get; set; }  // Mật khẩu đã mã hóa
        public string Email { get; set; }         // Email (unique)
        public string Phone { get; set; }         // SĐT (unique)
        public string FullName { get; set; }      // Tên hiển thị
        public int Status { get; set; }          // 0=Inactive,1=Active,2=Banned
        public int UserType { get; set; } = 2;        // 1=Người quản lý, 2=Người thi

        public List<long>? Roles { get; set; }        // Bắt buộc khi UserType = 1 (Người quản lý)

        public bool IsSuperAdmin { get; set; }
    }

    public class AppUserSearchModel : BaseSearch
    {
        public int? UserType { get; set; }
    }

    /// <summary>Admin reset mật khẩu user; để trống NewPassword → dùng mặc định 123456.</summary>
    public class ResetPasswordModel
    {
        public long Id { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
