using Example.API.Client.Clients.ExamPeriodAssignment;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade phân công kỳ thi — mọi thao tác đều ủy quyền cho <see cref="IExamPeriodAssignmentClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class ExamPeriodAssignmentRepository : IExamPeriodAssignmentRepository
    {
        // Client HTTP được inject qua DI để gọi API phân công kỳ thi
        private readonly IExamPeriodAssignmentClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public ExamPeriodAssignmentRepository(IExamPeriodAssignmentClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách phân công có phân trang/lọc qua <c>IExamPeriodAssignmentClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<ExamPeriodAssignmentModel>>> GetListAsync(ExamPeriodAssignmentSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một phân công theo id qua <c>IExamPeriodAssignmentClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<ExamPeriodAssignmentModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền thêm hàng loạt phân công qua <c>IExamPeriodAssignmentClient.AddBulkAsync</c>.</summary>
        public Task<ResponseData<object>> AddBulkAsync(ExamPeriodAssignmentBulkSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddBulkAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật một phân công qua <c>IExamPeriodAssignmentClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamPeriodAssignmentSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa phân công theo id qua <c>IExamPeriodAssignmentClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
