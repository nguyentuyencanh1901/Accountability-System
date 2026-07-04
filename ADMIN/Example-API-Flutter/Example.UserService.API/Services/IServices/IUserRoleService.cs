using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IUserRoleService
    {
        Task<ResponseData<IEnumerable<UserRoleModel>>> GetListPaging(UserRoleSearchModel search);
        Task<ResponseData<UserRoleModel>> GetById(long id);
        Task<ResponseData<object>> Insert(UserRoleSaveModel model);
        Task<ResponseData<object>> Update(UserRoleSaveModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
