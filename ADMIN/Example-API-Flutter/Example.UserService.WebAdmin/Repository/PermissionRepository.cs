using Example.API.Client.Clients.Permission;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade quyền hạn — mọi thao tác đều ủy quyền cho <see cref="IPermissionClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class PermissionRepository : IPermissionRepository
    {
        // Client HTTP được inject qua DI để gọi API quyền hạn
        private readonly IPermissionClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public PermissionRepository(IPermissionClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách quyền hạn có phân trang/lọc qua <c>IPermissionClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<PermissionModel>>> GetListAsync(PermissionSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một quyền hạn theo id qua <c>IPermissionClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<PermissionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo quyền hạn mới qua <c>IPermissionClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(PermissionSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật quyền hạn qua <c>IPermissionClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(PermissionSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa quyền hạn theo id qua <c>IPermissionClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
