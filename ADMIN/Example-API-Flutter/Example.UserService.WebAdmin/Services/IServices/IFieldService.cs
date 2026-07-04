using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.Field;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IFieldService
    {
        Task<(List<FieldModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex);
        FieldFormViewModel BuildCreateForm();
        Task<(FieldFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        Task<ResponseData<object>> InsertAsync(FieldFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(FieldFormViewModel model);
        Task<ResponseData<object>> UpdateStatusAsync(long id, int status);
        Task<ResponseData<object>> DeleteAsync(long id);
        void PopulateFormOptions(FieldFormViewModel model);
        FieldFormViewModel BuildFormViewModel(FieldFormViewModel model);
    }
}
