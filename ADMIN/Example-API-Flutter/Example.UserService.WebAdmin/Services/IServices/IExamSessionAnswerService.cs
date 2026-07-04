using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.ExamSessionAnswer;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IExamSessionAnswerService
    {
        Task<(List<ExamSessionAnswerModel> Items, long TotalItems, string? ErrorMessage, List<SelectListItem> ExamSessionFilterOptions, List<SelectListItem> QuestionFilterOptions)> GetListPagingAsync(int pageIndex, long? examSessionId, long? questionId);
        Task<ExamSessionAnswerFormViewModel> BuildCreateFormAsync();
        Task<(ExamSessionAnswerFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        Task<ResponseData<object>> InsertAsync(ExamSessionAnswerFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(ExamSessionAnswerFormViewModel model);
        Task<ResponseData<object>> DeleteAsync(long id);
        Task PopulateFormOptionsAsync(ExamSessionAnswerFormViewModel model);
        Task<List<SelectListItem>> LoadExamSessionOptionsAsync(long selectedId);
        Task<List<SelectListItem>> LoadExamSessionFilterOptionsAsync(long? selectedId);
        Task<List<SelectListItem>> LoadQuestionOptionsAsync(long selectedId);
        Task<List<SelectListItem>> LoadQuestionFilterOptionsAsync(long? selectedId);
        string FormatQuestionLabel(QuestionModel q);
    }
}
