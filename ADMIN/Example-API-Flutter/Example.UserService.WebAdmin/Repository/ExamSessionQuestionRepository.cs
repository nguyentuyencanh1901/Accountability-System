using Example.API.Client.Clients.ExamSessionQuestion;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade câu hỏi trong bài thi — mọi thao tác đều ủy quyền cho <see cref="IExamSessionQuestionClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class ExamSessionQuestionRepository : IExamSessionQuestionRepository
    {
        // Client HTTP được inject qua DI để gọi API câu hỏi bài thi
        private readonly IExamSessionQuestionClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public ExamSessionQuestionRepository(IExamSessionQuestionClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách câu hỏi bài thi có phân trang/lọc qua <c>IExamSessionQuestionClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<ExamSessionQuestionModel>>> GetListAsync(ExamSessionQuestionSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một câu hỏi bài thi theo id qua <c>IExamSessionQuestionClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<ExamSessionQuestionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo câu hỏi bài thi mới qua <c>IExamSessionQuestionClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(ExamSessionQuestionSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật câu hỏi bài thi qua <c>IExamSessionQuestionClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamSessionQuestionSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa câu hỏi bài thi theo id qua <c>IExamSessionQuestionClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
