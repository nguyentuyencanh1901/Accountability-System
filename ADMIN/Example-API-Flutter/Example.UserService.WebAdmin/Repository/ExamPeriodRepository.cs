using Example.API.Client.Clients.ExamPeriod;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Repository.IRepository;

namespace Example.UserService.WebAdmin.Repository
{
    /// <summary>
    /// Lớp facade kỳ thi — mọi thao tác đều ủy quyền cho <see cref="IExamPeriodClient"/> (HTTP API backend).
    /// Không chứa logic nghiệp vụ; chỉ chuyển tiếp request/response.
    /// </summary>
    public class ExamPeriodRepository : IExamPeriodRepository
    {
        // Client HTTP được inject qua DI để gọi API kỳ thi
        private readonly IExamPeriodClient _client;

        /// <summary>Khởi tạo repository với client API tương ứng.</summary>
        public ExamPeriodRepository(IExamPeriodClient client) => _client = client;

        /// <summary>Ủy quyền lấy danh sách kỳ thi có phân trang/lọc qua <c>IExamPeriodClient.GetListAsync</c>.</summary>
        public Task<ResponseData<List<ExamPeriodModel>>> GetListAsync(ExamPeriodSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        /// <summary>Ủy quyền lấy chi tiết một kỳ thi theo id qua <c>IExamPeriodClient.GetByIdAsync</c>.</summary>
        public Task<ResponseData<ExamPeriodModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        /// <summary>Ủy quyền lấy dữ liệu giám sát tiến độ thí sinh của kỳ thi qua <c>IExamPeriodClient.GetMonitoringAsync</c>.</summary>
        public Task<ResponseData<ExamPeriodMonitoringModel>> GetMonitoringAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetMonitoringAsync(id, cancellationToken);

        /// <summary>Ủy quyền tạo kỳ thi mới qua <c>IExamPeriodClient.AddAsync</c>.</summary>
        public Task<ResponseData<object>> AddAsync(ExamPeriodSaveModel model, CancellationToken cancellationToken = default)
            => _client.AddAsync(model, cancellationToken);

        /// <summary>Ủy quyền cập nhật kỳ thi qua <c>IExamPeriodClient.UpdateAsync</c>.</summary>
        public Task<ResponseData<object>> UpdateAsync(ExamPeriodSaveModel model, CancellationToken cancellationToken = default)
            => _client.UpdateAsync(model, cancellationToken);

        /// <summary>Ủy quyền xóa kỳ thi theo id qua <c>IExamPeriodClient.DeleteAsync</c>.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default)
            => _client.DeleteAsync(id, cancellationToken);
    }
}
