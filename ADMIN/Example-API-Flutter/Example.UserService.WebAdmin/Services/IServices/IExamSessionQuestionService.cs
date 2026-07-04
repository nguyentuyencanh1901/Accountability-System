using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.ExamSessionQuestion;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IExamSessionQuestionService
    {
        Task<(List<ExamSessionQuestionModel> Items, long TotalItems, string? ErrorMessage, List<SelectListItem> ExamSessionFilterOptions, List<SelectListItem> QuestionFilterOptions)> GetListPagingAsync(int pageIndex, long? examSessionId, long? questionId);
        Task<ExamSessionQuestionFormViewModel> BuildCreateFormAsync();
        Task<(ExamSessionQuestionFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        Task<ResponseData<object>> InsertAsync(ExamSessionQuestionFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(ExamSessionQuestionFormViewModel model);
        Task<ResponseData<object>> DeleteAsync(long id);
        Task PopulateFormOptionsAsync(ExamSessionQuestionFormViewModel model);
        Task<List<SelectListItem>> LoadExamSessionOptionsAsync(long selectedId);
        Task<List<SelectListItem>> LoadExamSessionFilterOptionsAsync(long? selectedId);
        Task<List<SelectListItem>> LoadQuestionOptionsAsync(long selectedId);
        Task<List<SelectListItem>> LoadQuestionFilterOptionsAsync(long? selectedId);
        string FormatExamSessionLabel(ExamSessionModel e);
        string FormatQuestionLabel(QuestionModel q);
    }
}
