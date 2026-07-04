using Example.API.Client.Clients.Authenticate;
using Example.API.Client.Models;
using Example.Common.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade xác thực — mọi thao tác đều ủy quyền cho <see cref="IAuthenticateClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class AuthenticateRepository : IAuthenticateRepository
    {
        // Client HTTP được inject qua DI để gọi API đăng nhập/đăng ký
        private readonly IAuthenticateClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public AuthenticateRepository(IAuthenticateClient client) => _client = client;

        /// <summary>Ủy quyền đăng nhập và nhận token qua <c>IAuthenticateClient.LoginAsync</c>.</summary>
        public Task<ResponseData<LoginResponse>> LoginAsync(LoginModel model, CancellationToken cancellationToken = default)
            => _client.LoginAsync(model, cancellationToken);

        /// <summary>Ủy quyền đăng ký tài khoản mới qua <c>IAuthenticateClient.RegisterAsync</c>.</summary>
        public Task<ResponseData<object>> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default)
            => _client.RegisterAsync(model, cancellationToken);

        public Task<ResponseData<object>> ChangePasswordAsync(ChangePasswordModel model, CancellationToken cancellationToken = default)
            => _client.ChangePasswordAsync(model, cancellationToken);

        public void Logout() => _client.Logout();
    }
}
