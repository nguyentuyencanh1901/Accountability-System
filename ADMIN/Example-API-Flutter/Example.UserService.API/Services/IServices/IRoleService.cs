using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IRoleService
    {
        Task<ResponseData<IEnumerable<RoleModel>>> GetListPaging(RoleSearchModel search);
        Task<ResponseData<RoleModel>> GetById(long id);
        Task<ResponseData<object>> Insert(RoleSaveModel model);
        Task<ResponseData<object>> Update(RoleSaveModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
