using Example.API.Client.Clients.ExamSet;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebApp.Repository.IRepository;

namespace Example.UserService.WebApp.Repository
{
    /// <summary>
    /// Facade đọc thông tin bộ đề thi từ API.
    /// Lớp này không kiểm tra trạng thái Active hay quyền truy cập — logic đó nằm ở Service.
    /// </summary>
    public class ExamSetRepository : IExamSetRepository
    {
        // Client HTTP gọi endpoint /exam-set trên backend.
        private readonly IExamSetClient _client;

        // Tiêm client qua DI; chi tiết endpoint nằm trong Example.API.Client.
        public ExamSetRepository(IExamSetClient client) => _client = client;

        // Ủy quyền lấy chi tiết bộ đề theo Id (tên, thời lượng, danh sách câu hỏi...).
        public Task<ResponseData<ExamSetModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);
    }
}
