using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.AnswerOption;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IAnswerOptionService
    {
        Task<(List<AnswerOptionModel> Items, long TotalItems, long FieldId, string FieldName, string QuestionContent, string? ErrorMessage, bool InvalidQuestion)> GetListPagingAsync(long questionId, int pageIndex);
        Task<(AnswerOptionFormViewModel? Model, string? ErrorMessage, bool InvalidQuestion)> BuildCreateFormAsync(long questionId);
        Task<(AnswerOptionFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        Task<ResponseData<object>> InsertAsync(AnswerOptionFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(AnswerOptionFormViewModel model);
        Task<ResponseData<object>> DeleteAsync(long id);
        Task<(long FieldId, string FieldName, string QuestionContent)?> LoadQuestionContextAsync(long questionId);
    }
}
