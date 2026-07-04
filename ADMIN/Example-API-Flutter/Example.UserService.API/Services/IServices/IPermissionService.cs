using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IPermissionService
    {
        Task<ResponseData<IEnumerable<PermissionModel>>> GetListPaging(PermissionSearchModel search);
        Task<ResponseData<PermissionModel>> GetById(long id);
        Task<ResponseData<object>> Insert(PermissionSaveModel model);
        Task<ResponseData<object>> Update(PermissionSaveModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
