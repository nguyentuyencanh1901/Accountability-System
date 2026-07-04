using Example.API.Client.Clients.Role;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade vai trò — mọi thao tác đều ủy quyền cho <see cref="IRoleClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        // Client HTTP được inject qua DI để gọi API vai trò
        private readonly IRoleClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public RoleRepository(IRoleClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách vai trò có phân trang/lọc qua <c>IRoleClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<RoleModel>>> GetListAsync(RoleSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một vai trò theo id qua <c>IRoleClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<RoleModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo vai trò mới qua <c>IRoleClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(RoleSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật vai trò qua <c>IRoleClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(RoleSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa vai trò theo id qua <c>IRoleClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
