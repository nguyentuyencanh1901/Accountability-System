using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IRolePermissionService
    {
        Task<ResponseData<IEnumerable<RolePermissionModel>>> GetListPaging(RolePermissionSearchModel search);
        Task<ResponseData<RolePermissionModel>> GetById(long id);
        Task<ResponseData<object>> Insert(RolePermissionSaveModel model);
        Task<ResponseData<object>> Update(RolePermissionSaveModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
