namespace Example.API.Client.Models
{
    /// <summary>DTO trả về từ POST /Authenticate/login</summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public LoginUserResponse User { get; set; } = new();
    }

    public class LoginUserResponse
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int UserType { get; set; }
    }
}
