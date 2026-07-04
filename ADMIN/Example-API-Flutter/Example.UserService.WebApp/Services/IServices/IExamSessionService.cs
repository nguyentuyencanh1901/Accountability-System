using Example.UserService.API.Models;
using Example.UserService.WebApp.Models.ExamSession;

namespace Example.UserService.WebApp.Services.IServices
{
    public class StartExamServiceResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public long SessionId { get; set; }
        public string? ResumeMessage { get; set; }
        public long RedirectExamSetId { get; set; }
        public long ExamPeriodAssignmentId { get; set; }
    }

    public interface IExamSessionService
    {
        Task<(List<ExamSessionModel> Items, long TotalItems, string? ErrorMessage)> GetInProgressListAsync(long userId, int pageIndex);
        Task<(List<ExamSessionModel> Items, long TotalItems, string? ErrorMessage)> GetHistoryListAsync(long userId, int pageIndex);
        Task<(ExamSessionModel? Session, string? ErrorMessage)> GetOwnedSessionAsync(long id, long userId);
        Task<StartExamServiceResult> StartExamAsync(long examSetId, int examType, long examPeriodAssignmentId, long userId);
        Task<(bool Success, string? Message)> SaveProgressAsync(SubmitExamFormModel model, long userId);
        Task<(TakeExamViewModel? ViewModel, string? SuccessMessage, string? ErrorMessage, bool RedirectToDetails, long SessionId)> GetTakeExamAsync(long id, long userId);
        Task<(bool Success, string? ErrorMessage, long SessionId, bool RedirectToDetails, bool ShowSuccessMessage)> SubmitExamAsync(SubmitExamFormModel model, long userId);
        Task<(bool Success, string? ErrorMessage, long SessionId)> CancelDueToViolationAsync(SubmitExamFormModel model, long userId);
    }
}
