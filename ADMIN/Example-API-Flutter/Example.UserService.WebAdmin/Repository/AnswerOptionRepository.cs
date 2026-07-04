using Example.API.Client.Clients.AnswerOption;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade đáp án câu hỏi — mọi thao tác đều ủy quyền cho <see cref="IAnswerOptionClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class AnswerOptionRepository : IAnswerOptionRepository
    {
        // Client HTTP được inject qua DI để gọi API đáp án
        private readonly IAnswerOptionClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public AnswerOptionRepository(IAnswerOptionClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách đáp án có phân trang/lọc qua <c>IAnswerOptionClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<AnswerOptionModel>>> GetListAsync(AnswerOptionSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một đáp án theo id qua <c>IAnswerOptionClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<AnswerOptionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo đáp án mới qua <c>IAnswerOptionClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(AnswerOptionSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật đáp án qua <c>IAnswerOptionClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(AnswerOptionSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa đáp án theo id qua <c>IAnswerOptionClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
