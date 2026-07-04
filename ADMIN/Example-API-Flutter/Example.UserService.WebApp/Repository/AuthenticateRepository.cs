using Example.API.Client.Clients.Authenticate;
using Example.API.Client.Models;
using Example.Common.Models;
using Example.UserService.WebApp.Repository.IRepository;

namespace Example.UserService.WebApp.Repository
{
    /// <summary>
    /// Lớp facade (mặt tiền) truy cập dữ liệu xác thực cho WebApp thí sinh.
    /// Không chứa logic nghiệp vụ — mọi thao tác đều ủy quyền trực tiếp sang <see cref="IAuthenticateClient"/> (HTTP client gọi UserService.API).
    /// </summary>
    public class AuthenticateRepository : IAuthenticateRepository
    {
        // Client HTTP đã cấu hình base URL, JWT header; Repository chỉ chuyển tiếp lời gọi.
        private readonly IAuthenticateClient _client;

        // Tiêm client qua DI để dễ mock khi test và tách biệt WebApp khỏi chi tiết HTTP.
        public AuthenticateRepository(IAuthenticateClient client) => _client = client;

        // Ủy quyền đăng nhập endpoint login-app (trả JWT + thông tin thí sinh).
        public Task<ResponseData<LoginResponse>> LoginAppAsync(LoginModel model, CancellationToken cancellationToken = default)
            => _client.LoginAppAsync(model, cancellationToken);

        // Ủy quyền đăng ký tài khoản thí sinh mới lên API.
        public Task<ResponseData<object>> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default)
            => _client.RegisterAsync(model, cancellationToken);

        // Ủy quyền cập nhật hồ sơ self-service (họ tên, email, SĐT) — API xác thực theo JWT hiện tại.
        public Task<ResponseData<object>> UpdateProfileAsync(UpdateProfileModel model, CancellationToken cancellationToken = default)
            => _client.UpdateProfileAsync(model, cancellationToken);

        // Ủy quyền đổi mật khẩu; API kiểm tra mật khẩu hiện tại và xác nhận mật khẩu mới.
        public Task<ResponseData<object>> ChangePasswordAsync(ChangePasswordModel model, CancellationToken cancellationToken = default)
            => _client.ChangePasswordAsync(model, cancellationToken);

        // Xóa token/cookie phiên đăng nhập phía client (không gọi API server).
        public void Logout() => _client.Logout();
    }
}
