using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.ExamSessionAnswerOption;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IExamSessionAnswerOptionService
    {
        Task<(List<ExamSessionAnswerOptionModel> Items, long TotalItems, string? ErrorMessage, List<SelectListItem> ExamSessionAnswerFilterOptions, List<SelectListItem> AnswerOptionFilterOptions)> GetListPagingAsync(int pageIndex, long? examSessionAnswerId, long? answerOptionId);
        Task<ExamSessionAnswerOptionFormViewModel> BuildCreateFormAsync();
        Task<(ExamSessionAnswerOptionFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        Task<ResponseData<object>> InsertAsync(ExamSessionAnswerOptionFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(ExamSessionAnswerOptionFormViewModel model);
        Task<ResponseData<object>> DeleteAsync(long id);
        Task PopulateFormOptionsAsync(ExamSessionAnswerOptionFormViewModel model);
        Task<List<SelectListItem>> LoadExamSessionAnswerOptionsAsync(long selectedId);
        Task<List<SelectListItem>> LoadExamSessionAnswerFilterOptionsAsync(long? selectedId);
        Task<List<SelectListItem>> LoadAnswerOptionOptionsAsync(long selectedId);
        Task<List<SelectListItem>> LoadAnswerOptionFilterOptionsAsync(long? selectedId);
        string FormatExamSessionAnswerLabel(ExamSessionAnswerModel a);
        string FormatAnswerOptionLabel(AnswerOptionModel a);
    }
}
