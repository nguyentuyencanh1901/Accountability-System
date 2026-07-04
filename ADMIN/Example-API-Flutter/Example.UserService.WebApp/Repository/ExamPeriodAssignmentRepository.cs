using Example.API.Client.Clients.ExamPeriodAssignment;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebApp.Repository.IRepository;

namespace Example.UserService.WebApp.Repository
{
    /// <summary>
    /// Facade truy vấn phân công kỳ thi của thí sinh đang đăng nhập.
    /// Không xử lý quy tắc hiển thị hay thời gian thi — chỉ ủy quyền sang <see cref="IExamPeriodAssignmentClient"/>.
    /// </summary>
    public class ExamPeriodAssignmentRepository : IExamPeriodAssignmentRepository
    {
        // Client HTTP gọi endpoint phân công kỳ thi trên UserService.API.
        private readonly IExamPeriodAssignmentClient _client;

        // Tiêm client qua DI; API lọc phân công theo user từ JWT.
        public ExamPeriodAssignmentRepository(IExamPeriodAssignmentClient client) => _client = client;

        // Ủy quyền lấy danh sách phân công thuộc user hiện tại (API lọc theo JWT).
        public Task<ResponseData<List<ExamPeriodAssignmentModel>>> GetMyAssignmentsAsync(CancellationToken cancellationToken = default)
            => _client.GetMyAssignmentsAsync(cancellationToken);
    }
}
