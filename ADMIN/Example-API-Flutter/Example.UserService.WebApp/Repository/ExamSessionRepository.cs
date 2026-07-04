using Example.API.Client.Clients.ExamSession;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebApp.Repository.IRepository;

namespace Example.UserService.WebApp.Repository
{
    /// <summary>
    /// Facade quản lý phiên thi (exam session) — toàn bộ thao tác CRUD/làm bài/nộp bài
    /// được ủy quyền sang <see cref="IExamSessionClient"/> gọi UserService.API.
    /// </summary>
    public class ExamSessionRepository : IExamSessionRepository
    {
        // Client HTTP đã gắn JWT thí sinh; Repository không can thiệp vào payload hay phân quyền.
        private readonly IExamSessionClient _client;

        // Tiêm client qua DI; Repository chỉ chuyển tiếp, không xử lý HTTP/JWT.
        public ExamSessionRepository(IExamSessionClient client) => _client = client;

        // Ủy quyền tìm kiếm phiên thi (đang làm, theo user, bộ đề, loại thi...).
        public Task<ResponseData<List<ExamSessionModel>>> GetListAsync(ExamSessionSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetListAsync(search, cancellationToken);

        // Ủy quyền lấy lịch sử thi đã hoàn thành/hết hạn (endpoint riêng trên API).
        public Task<ResponseData<List<ExamSessionModel>>> GetHistoryAsync(ExamSessionSearchModel search, CancellationToken cancellationToken = default)
            => _client.GetHistoryAsync(search, cancellationToken);

        // Ủy quyền lấy chi tiết một phiên thi theo Id.
        public Task<ResponseData<ExamSessionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _client.GetByIdAsync(id, cancellationToken);

        // Ủy quyền bắt đầu hoặc tiếp tục bài thi; API xử lý random bộ đề, ghi session InProgress.
        public Task<ResponseData<ExamSessionModel>> StartExamAsync(StartExamModel model, CancellationToken cancellationToken = default)
            => _client.StartExamAsync(model, cancellationToken);

        // Ủy quyền lưu tiến độ tạm (đáp án chưa nộp) — không chấm điểm.
        public Task<ResponseData<object>> SaveExamProgressAsync(SubmitExamModel model, CancellationToken cancellationToken = default)
            => _client.SaveExamProgressAsync(model, cancellationToken);

        // Ủy quyền nộp bài chính thức; API chấm điểm và đổi trạng thái session.
        public Task<ResponseData<ExamSessionModel>> SubmitExamAsync(SubmitExamModel model, CancellationToken cancellationToken = default)
            => _client.SubmitExamAsync(model, cancellationToken);

        public Task<ResponseData<object>> CancelExamDueToViolationAsync(SubmitExamModel model, CancellationToken cancellationToken = default)
            => _client.CancelExamDueToViolationAsync(model, cancellationToken);
    }
}
