using Example.API.Client.Configuration;
using Example.Common.Enums;
using Example.UserService.API.Models;
using Example.UserService.WebApp.Helpers;
using Example.UserService.WebApp.Models.ExamSession;
using Example.UserService.WebApp.Repository.IRepository;
using Example.UserService.WebApp.Services.IServices;
using Microsoft.Extensions.Options;

namespace Example.UserService.WebApp.Services
{
    /// <summary>
    /// Dịch vụ phiên thi: danh sách đang làm/lịch sử, bắt đầu/tiếp tục bài, lưu tiến độ, nộp bài.
    /// Chứa quy tắc phân quyền (chỉ owner), kiểm tra trạng thái session và chuẩn hóa đáp án trước khi gọi API.
    /// </summary>
    public class ExamSessionService : IExamSessionService
    {
        private readonly IExamSessionRepository _sessionRepository;
        private readonly IExamSetRepository _examSetRepository;
        private readonly string _apiBaseUrl;

        // Tiêm repository và cấu hình API base URL (dùng cho màn làm bài / upload ảnh).
        public ExamSessionService(
            IExamSessionRepository sessionRepository,
            IExamSetRepository examSetRepository,
            IOptions<ApiClientOptions> apiOptions)
        {
            _sessionRepository = sessionRepository;
            _examSetRepository = examSetRepository;
            _apiBaseUrl = apiOptions.Value.BaseUrl ?? string.Empty;
        }

        /// <summary>Danh sách phiên thi đang InProgress của user (phân trang 10 bản ghi/trang).</summary>
        public async Task<(List<ExamSessionModel> Items, long TotalItems, string? ErrorMessage)> GetInProgressListAsync(long userId, int pageIndex)
        {
            var result = await _sessionRepository.GetListAsync(new ExamSessionSearchModel
            {
                PageIndex = pageIndex,
                PageSize = 10,
                UserId = userId,
                Status = (int)ExamSessionStatusEnum.InProgress
            });
            return ApiListHelper.ParseList(result);
        }

        /// <summary>Lịch sử thi đã hoàn thành/hết hạn (endpoint GetHistory trên API).</summary>
        public async Task<(List<ExamSessionModel> Items, long TotalItems, string? ErrorMessage)> GetHistoryListAsync(long userId, int pageIndex)
        {
            var result = await _sessionRepository.GetHistoryAsync(new ExamSessionSearchModel
            {
                PageIndex = pageIndex,
                PageSize = 10,
                UserId = userId
            });
            return ApiListHelper.ParseList(result);
        }

        /// <summary>
        /// Lấy phiên thi theo Id và xác minh quyền sở hữu — userId > 0 phải khớp session.UserId.
        /// </summary>
        public async Task<(ExamSessionModel? Session, string? ErrorMessage)> GetOwnedSessionAsync(long id, long userId)
        {
            var result = await _sessionRepository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy phiên thi.");

            // Quy tắc nghiệp vụ: chặn truy cập phiên thi của người khác.
            if (userId > 0 && result.Data.UserId != userId)
                return (null, "Bạn không có quyền truy cập phiên thi này.");

            return (result.Data, null);
        }

        /// <summary>
        /// Bắt đầu hoặc tiếp tục bài thi: kiểm tra bộ đề (nếu không qua phân công), tìm session InProgress cũ,
        /// rồi gọi API StartExam. Trả ResumeMessage khi API trả về cùng session đang làm.
        /// </summary>
        public async Task<StartExamServiceResult> StartExamAsync(long examSetId, int examType, long examPeriodAssignmentId, long userId)
        {
            var redirectExamSetId = examSetId;
            // Quy tắc nghiệp vụ: thi tự do (không có phân công) phải xác minh bộ đề tồn tại và Active.
            if (examPeriodAssignmentId <= 0)
            {
                var examSetResult = await _examSetRepository.GetByIdAsync(examSetId);
                if (!examSetResult.Success || examSetResult.Data == null || examSetResult.Data.Status != (int)StatusEnum.Active)
                {
                    return new StartExamServiceResult
                    {
                        Success = false,
                        ErrorMessage = "Bộ đề không khả dụng.",
                        RedirectExamSetId = examSetId,
                        ExamPeriodAssignmentId = examPeriodAssignmentId
                    };
                }
            }

            long? existingSessionId = null;
            // Kiểm tra đã có bài đang làm cùng bộ đề + loại thi chưa (để hiển thị thông báo tiếp tục).
            var existingResult = await _sessionRepository.GetListAsync(new ExamSessionSearchModel
            {
                PageIndex = 1,
                PageSize = 1,
                UserId = userId,
                ExamSetId = examSetId > 0 ? examSetId : null,
                ExamType = examType,
                Status = (int)ExamSessionStatusEnum.InProgress
            });
            existingSessionId = existingResult.Data?.FirstOrDefault()?.Id;

            var result = await _sessionRepository.StartExamAsync(new StartExamModel
            {
                ExamPeriodAssignmentId = examPeriodAssignmentId,
                ExamSetId = examSetId,
                ExamType = examType
            });

            if (!result.Success || result.Data == null)
            {
                return new StartExamServiceResult
                {
                    Success = false,
                    ErrorMessage = result.Message ?? "Không thể bắt đầu bài thi.",
                    RedirectExamSetId = redirectExamSetId,
                    ExamPeriodAssignmentId = examPeriodAssignmentId
                };
            }

            return new StartExamServiceResult
            {
                Success = true,
                SessionId = result.Data.Id,
                // Thông báo tiếp tục khi API trả về đúng session InProgress đã có từ trước.
                ResumeMessage = existingSessionId.HasValue && existingSessionId.Value == result.Data.Id
                    ? "Tiếp tục bài thi đang làm."
                    : null,
                RedirectExamSetId = redirectExamSetId,
                ExamPeriodAssignmentId = examPeriodAssignmentId
            };
        }

        /// <summary>Lưu đáp án tạm: chỉ cho phép khi session InProgress và thuộc user.</summary>
        public async Task<(bool Success, string? Message)> SaveProgressAsync(SubmitExamFormModel model, long userId)
        {
            // Lưu đáp án tạm lên API, không chấm điểm.
            if (model.ExamSessionId <= 0)
                return (false, "Phiên thi không hợp lệ.");

            var (session, error) = await GetOwnedSessionAsync(model.ExamSessionId, userId);
            if (session == null)
                return (false, error ?? "Phiên thi không hợp lệ.");
            // Quy tắc nghiệp vụ: không lưu tiến độ khi session đã kết thúc.
            if (session.Status != (int)ExamSessionStatusEnum.InProgress)
                return (false, "Phiên thi đã kết thúc.");

            var result = await _sessionRepository.SaveExamProgressAsync(new SubmitExamModel
            {
                ExamSessionId = model.ExamSessionId,
                Answers = BuildSubmitAnswers(model)
            });

            return (result.Success, result.Message);
        }

        /// <summary>
        /// Chuẩn bị màn làm bài: kiểm tra quyền/trạng thái, tính thời gian kết thúc cho đếm ngược JS.
        /// </summary>
        public async Task<(TakeExamViewModel? ViewModel, string? SuccessMessage, string? ErrorMessage, bool RedirectToDetails, long SessionId)> GetTakeExamAsync(long id, long userId)
        {
            var (session, error) = await GetOwnedSessionAsync(id, userId);
            if (session == null)
                return (null, null, error, false, id);

            // Session đã hết hạn, đã nộp hoặc bị hủy — chuyển về trang chi tiết thay vì màn làm bài.
            if (session.Status != (int)ExamSessionStatusEnum.InProgress)
            {
                var successMessage = session.Status switch
                {
                    (int)ExamSessionStatusEnum.Expired => "Bài thi đã hết giờ và được tính điểm tự động.",
                    (int)ExamSessionStatusEnum.Cancelled => "Bài thi đã bị hủy do vi phạm quy chế thi.",
                    _ => null
                };
                return (null, successMessage, null, true, id);
            }

            var examSetResult = await _examSetRepository.GetByIdAsync(session.ExamSetId);
            var duration = examSetResult.Data?.DurationMinutes ?? 60;
            var startedAt = session.StartedAt == default ? DateTimeOffset.Now : session.StartedAt;

            // EndTimeUnixMs dùng cho đếm ngược JavaScript trên màn làm bài.
            return (new TakeExamViewModel
            {
                Session = session,
                DurationMinutes = duration,
                EndTimeUnixMs = startedAt.AddMinutes(duration).ToUnixTimeMilliseconds(),
                ApiBaseUrl = _apiBaseUrl
            }, null, null, false, id);
        }

        /// <summary>
        /// Nộp bài chính thức: nếu session đã kết thúc thì coi như thành công và redirect;
        /// ngược lại gọi API SubmitExam để chấm điểm.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage, long SessionId, bool RedirectToDetails, bool ShowSuccessMessage)> SubmitExamAsync(SubmitExamFormModel model, long userId)
        {
            if (model.ExamSessionId <= 0)
                return (false, "Phiên thi không hợp lệ.", model.ExamSessionId, false, false);

            var (session, error) = await GetOwnedSessionAsync(model.ExamSessionId, userId);
            if (session == null)
                return (false, error, model.ExamSessionId, false, false);

            // Idempotent: nộp lại khi đã kết thúc vẫn redirect thành công, không gọi API lần nữa.
            if (session.Status != (int)ExamSessionStatusEnum.InProgress)
                return (true, null, model.ExamSessionId, true, false);

            var result = await _sessionRepository.SubmitExamAsync(new SubmitExamModel
            {
                ExamSessionId = model.ExamSessionId,
                Answers = BuildSubmitAnswers(model)
            });

            if (!result.Success)
                return (false, result.Message ?? "Nộp bài thất bại.", model.ExamSessionId, false, false);

            return (true, null, model.ExamSessionId, true, true);
        }

        /// <summary>
        /// Hủy bài thi do vi phạm quy chế: gọi API cancel-exam-violation, khóa không cho thi lại.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage, long SessionId)> CancelDueToViolationAsync(SubmitExamFormModel model, long userId)
        {
            if (model.ExamSessionId <= 0)
                return (false, "Phiên thi không hợp lệ.", model.ExamSessionId);

            var (session, error) = await GetOwnedSessionAsync(model.ExamSessionId, userId);
            if (session == null)
                return (false, error, model.ExamSessionId);

            // Idempotent: đã kết thúc thì coi như thành công
            if (session.Status != (int)ExamSessionStatusEnum.InProgress)
                return (true, null, model.ExamSessionId);

            var result = await _sessionRepository.CancelExamDueToViolationAsync(new SubmitExamModel
            {
                ExamSessionId = model.ExamSessionId,
                ViolationCount = model.ViolationCount,
                Answers = BuildSubmitAnswers(model)
            });

            if (!result.Success)
                return (false, result.Message ?? "Không thể hủy bài thi.", model.ExamSessionId);

            return (true, null, model.ExamSessionId);
        }

        /// <summary>
        /// Chuẩn hóa đáp án từ form trước khi gửi API: loại id không hợp lệ, trùng lặp, và câu chưa chọn đáp án.
        /// </summary>
        private static List<SubmitAnswerModel> BuildSubmitAnswers(SubmitExamFormModel model)
        {
            // Chỉ gửi câu đã chọn ít nhất một đáp án; bỏ id <= 0 và trùng.
            return model.Answers?
                .Select(a => new SubmitAnswerModel
                {
                    QuestionId = a.QuestionId,
                    SelectedAnswerOptionIds = (a.SelectedAnswerOptionIds ?? new List<long>())
                        .Where(id => id > 0)
                        .Distinct()
                        .ToList()
                })
                .Where(a => a.SelectedAnswerOptionIds.Count > 0)
                .ToList() ?? new List<SubmitAnswerModel>();
        }
    }
}
