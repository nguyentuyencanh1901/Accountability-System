using Example.API.Client.Clients.ExamSet;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade bộ đề — mọi thao tác đều ủy quyền cho <see cref="IExamSetClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class ExamSetRepository : IExamSetRepository
    {
        // Client HTTP được inject qua DI để gọi API bộ đề
        private readonly IExamSetClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public ExamSetRepository(IExamSetClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách bộ đề có phân trang/lọc qua <c>IExamSetClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<ExamSetModel>>> GetListAsync(ExamSetSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một bộ đề theo id qua <c>IExamSetClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<ExamSetModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo bộ đề mới qua <c>IExamSetClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(ExamSetSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật bộ đề qua <c>IExamSetClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamSetSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa bộ đề theo id qua <c>IExamSetClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);

        /// <summary>Ủy quyền lấy danh sách thí sinh đã làm bộ đề qua <c>IExamSetClient.GetExamTakersAsync</c>.</summary>
        public Task<ResponseData<List<ExamSetTakerModel>>> GetExamTakersAsync(ExamSetTakerSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetExamTakersAsync(search, cancellationToken);
    }
}
