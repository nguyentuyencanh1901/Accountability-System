using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Models.Role;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IRoleService
    {
        Task<(List<RoleModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex);
        Task<RoleFormViewModel> BuildCreateFormAsync();
        Task<(RoleFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id);
        Task<ResponseData<object>> InsertAsync(RoleFormViewModel model);
        Task<ResponseData<object>> UpdateAsync(RoleFormViewModel model);
        Task<ResponseData<object>> DeleteAsync(long id);
        Task PopulateFormOptionsAsync(RoleFormViewModel model);
    }
}
