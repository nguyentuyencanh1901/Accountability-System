using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.ExamSet;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IExamSetService
    {
        Task<(List<ExamSetModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex);
        Task<ExamSetFormViewModel> BuildCreateFormAsync();
        Task<(ExamSetFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        Task<ResponseData<object>> InsertAsync(ExamSetFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(ExamSetFormViewModel model);
        Task<ResponseData<object>> UpdateStatusAsync(long id, int status);
        Task<ResponseData<object>> DeleteAsync(long id);
        void PopulateFormOptions(ExamSetFormViewModel model);
        Task PopulateFieldOptionsAsync(ExamSetFormViewModel model);
        ExamSetFormViewModel BuildFormViewModel(ExamSetFormViewModel model);
        Task<(ExamSetModel? ExamSet, List<ExamSetTakerModel> Items, long TotalItems, string? ErrorMessage)> GetExamTakersAsync(long examSetId, int pageIndex);
        string GetExamTypeName(int examType);
        string GetSessionStatusName(int status);
    }
}
