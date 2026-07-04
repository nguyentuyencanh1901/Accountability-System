using Example.UserService.WebApp.Models.ExamSet;
using Example.UserService.API.Models;

namespace Example.UserService.WebApp.Services.IServices
{
    public interface IExamSetService
    {
        Task<ExamSetIndexViewModel> GetIndexAsync();
        Task<(ExamSetDetailsViewModel? Model, string? ErrorMessage)> GetDetailsAsync(long assignmentId, long userId);
        string GetExamTypeName(int examType);
        string GetAssignmentStatusName(int status);
        string GetExamPeriodStatusName(ExamPeriodAssignmentModel assignment);
        string GetDisplayStatusName(ExamPeriodAssignmentModel assignment);
        bool CanStartExam(ExamPeriodAssignmentModel assignment);
        bool CanContinueExam(ExamPeriodAssignmentModel assignment);
    }
}
