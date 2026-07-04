using Example.API.Client.Clients.Question;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade câu hỏi — mọi thao tác đều ủy quyền cho <see cref="IQuestionClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class QuestionRepository : IQuestionRepository
    {
        // Client HTTP được inject qua DI để gọi API câu hỏi
        private readonly IQuestionClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public QuestionRepository(IQuestionClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách câu hỏi có phân trang/lọc qua <c>IQuestionClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<QuestionModel>>> GetListAsync(QuestionSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một câu hỏi theo id qua <c>IQuestionClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<QuestionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo câu hỏi mới qua <c>IQuestionClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(QuestionSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật câu hỏi qua <c>IQuestionClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(QuestionSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật nhanh trạng thái qua <c>IQuestionClient.UpdateStatusAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateStatusAsync(UpdateStatusModel model, CancellationToken cancellationToken = default)
            => _client.UpdateStatusAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa câu hỏi theo id qua <c>IQuestionClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
