using Example.API.Client.Clients.ExamSession;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade phiên/bài thi — mọi thao tác đều ủy quyền cho <see cref="IExamSessionClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class ExamSessionRepository : IExamSessionRepository
    {
        // Client HTTP được inject qua DI để gọi API bài thi
        private readonly IExamSessionClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public ExamSessionRepository(IExamSessionClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách bài thi có phân trang/lọc qua <c>IExamSessionClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<ExamSessionModel>>> GetListAsync(ExamSessionSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một bài thi theo id qua <c>IExamSessionClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<ExamSessionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo bài thi mới qua <c>IExamSessionClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(ExamSessionSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật bài thi qua <c>IExamSessionClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamSessionSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa bài thi theo id qua <c>IExamSessionClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
