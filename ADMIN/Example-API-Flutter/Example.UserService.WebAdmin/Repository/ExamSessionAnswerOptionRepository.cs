using Example.API.Client.Clients.ExamSessionAnswerOption;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade đáp án đã chọn trong bài thi — mọi thao tác đều ủy quyền cho <see cref="IExamSessionAnswerOptionClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class ExamSessionAnswerOptionRepository : IExamSessionAnswerOptionRepository
    {
        // Client HTTP được inject qua DI để gọi API đáp án đã chọn
        private readonly IExamSessionAnswerOptionClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public ExamSessionAnswerOptionRepository(IExamSessionAnswerOptionClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách đáp án đã chọn có phân trang/lọc qua <c>IExamSessionAnswerOptionClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<ExamSessionAnswerOptionModel>>> GetListAsync(ExamSessionAnswerOptionSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một bản ghi theo id qua <c>IExamSessionAnswerOptionClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<ExamSessionAnswerOptionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo bản ghi mới qua <c>IExamSessionAnswerOptionClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(ExamSessionAnswerOptionSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật bản ghi qua <c>IExamSessionAnswerOptionClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamSessionAnswerOptionSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa bản ghi theo id qua <c>IExamSessionAnswerOptionClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
