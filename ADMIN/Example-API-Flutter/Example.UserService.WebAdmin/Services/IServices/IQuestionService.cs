using Example.Common.Models;
using Example.UserService.WebAdmin.Models.Question;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IQuestionService
    {
        Task<QuestionIndexViewModel> GetListPagingAsync(QuestionIndexViewModel filter);
        Task<QuestionFormViewModel> BuildCreateFormAsync();
        Task<(QuestionFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        Task<ResponseData<object>> InsertAsync(QuestionFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(QuestionFormViewModel model);
        Task<ResponseData<object>> UpdateStatusAsync(long id, int status);
        Task<ResponseData<object>> DeleteAsync(long id);
        void PopulateFormOptions(QuestionFormViewModel model);
        Task PopulateDropdownOptionsAsync(QuestionFormViewModel model);
        void ApplyImagePreview(QuestionFormViewModel model);
        QuestionFormViewModel BuildFormViewModel(QuestionFormViewModel model);
    }
}
