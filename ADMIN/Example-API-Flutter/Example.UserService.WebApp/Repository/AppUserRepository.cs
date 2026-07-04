using Example.API.Client.Clients.AppUser;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebApp.Repository.IRepository;

namespace Example.UserService.WebApp.Repository
{
    /// <summary>
    /// Facade đọc thông tin người dùng từ API — WebApp không truy cập DB trực tiếp.
    /// Mọi phương thức chuyển tiếp sang <see cref="IAppUserClient"/> tương ứng endpoint UserService.API.
    /// </summary>
    public class AppUserRepository : IAppUserRepository
    {
        // Client HTTP gọi các endpoint /app-user trên backend.
        private readonly IAppUserClient _client;

        // Tiêm client qua DI; WebApp không truy cập DB user trực tiếp.
        public AppUserRepository(IAppUserClient client) => _client = client;

        // Ủy quyền lấy chi tiết user theo Id (dùng hiển thị trang hồ sơ thí sinh).
        public Task<ResponseData<AppUserModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);
    }
}
