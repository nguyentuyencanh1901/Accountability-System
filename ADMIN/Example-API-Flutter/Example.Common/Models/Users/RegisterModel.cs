using System.ComponentModel.DataAnnotations;

namespace Example.Common.Models
{
    public class RegisterModel
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string? Phone { get; set; }
        public string? FullName { get; set; }
        public int Status { get; set; } = 1;
        public int UserType { get; set; } = 2;   // 1=Người quản lý, 2=Người thi

        public List<long> Roles { get; set; }
    }
}
