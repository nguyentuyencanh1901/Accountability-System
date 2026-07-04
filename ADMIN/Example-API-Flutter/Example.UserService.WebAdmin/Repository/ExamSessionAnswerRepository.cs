using Example.API.Client.Clients.ExamSessionAnswer;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade câu trả lời bài thi — mọi thao tác đều ủy quyền cho <see cref="IExamSessionAnswerClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class ExamSessionAnswerRepository : IExamSessionAnswerRepository
    {
        // Client HTTP được inject qua DI để gọi API câu trả lời bài thi
        private readonly IExamSessionAnswerClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public ExamSessionAnswerRepository(IExamSessionAnswerClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách câu trả lời có phân trang/lọc qua <c>IExamSessionAnswerClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<ExamSessionAnswerModel>>> GetListAsync(ExamSessionAnswerSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một câu trả lời theo id qua <c>IExamSessionAnswerClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<ExamSessionAnswerModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo câu trả lời mới qua <c>IExamSessionAnswerClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(ExamSessionAnswerSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật câu trả lời qua <c>IExamSessionAnswerClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamSessionAnswerSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa câu trả lời theo id qua <c>IExamSessionAnswerClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
