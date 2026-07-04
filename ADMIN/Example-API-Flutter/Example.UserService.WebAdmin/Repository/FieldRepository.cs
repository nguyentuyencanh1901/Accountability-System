using Example.API.Client.Clients.Field;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade lĩnh vực — mọi thao tác đều ủy quyền cho <see cref="IFieldClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class FieldRepository : IFieldRepository
    {
        // Client HTTP được inject qua DI để gọi API lĩnh vực
        private readonly IFieldClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public FieldRepository(IFieldClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách lĩnh vực có phân trang/lọc qua <c>IFieldClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<FieldModel>>> GetListAsync(FieldSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một lĩnh vực theo id qua <c>IFieldClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<FieldModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo lĩnh vực mới qua <c>IFieldClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(FieldSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật lĩnh vực qua <c>IFieldClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(FieldSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật nhanh trạng thái qua <c>IFieldClient.UpdateStatusAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateStatusAsync(UpdateStatusModel model, CancellationToken cancellationToken = default)
            => _client.UpdateStatusAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa lĩnh vực theo id qua <c>IFieldClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
