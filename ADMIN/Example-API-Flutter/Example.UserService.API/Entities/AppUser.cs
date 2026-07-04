using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    // Bảng gốc của hệ thống
    public class AppUser : EntityAuditBase<long>
    {
        public string Username { get; set; }      // Tên đăng nhập (unique)
        public string PasswordHash { get; set; }  // Mật khẩu đã mã hóa
        public string Email { get; set; }         // Email (unique)
        public string Phone { get; set; }         // SĐT (unique)
        public string FullName { get; set; }      // Tên hiển thị
        public int Status { get; set; }          // 0=Inactive,1=Active,2=Banned
        public int UserType { get; set; }        // 1=Người quản lý, 2=Người thi
    }
}
