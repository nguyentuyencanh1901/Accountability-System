using Example.API.Client.Clients.AppUser;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade người dùng — mọi thao tác đều ủy quyền cho <see cref="IAppUserClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class AppUserRepository : IAppUserRepository
    {
        // Client HTTP được inject qua DI để gọi API người dùng
        private readonly IAppUserClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public AppUserRepository(IAppUserClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách người dùng có phân trang/lọc qua <c>IAppUserClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<AppUserModel>>> GetListAsync(AppUserSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một người dùng theo id qua <c>IAppUserClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<AppUserModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo người dùng mới qua <c>IAppUserClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(AppUserSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật người dùng qua <c>IAppUserClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(AppUserSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền reset mật khẩu qua <c>IAppUserClient.ResetPasswordAsync</c>.</summary>
        public Task<ResponseData<object>> ResetPasswordAsync(ResetPasswordModel model, CancellationToken cancellationToken = default)
            => _client.ResetPasswordAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa người dùng theo id qua <c>IAppUserClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
