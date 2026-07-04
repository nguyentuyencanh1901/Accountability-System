using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IAppUserService
    {
        Task<ResponseData<IEnumerable<AppUserModel>>> GetListPaging(AppUserSearchModel search);
        Task<ResponseData<AppUserModel>> GetById(long id);
        Task<ResponseData<object>> Insert(AppUserSaveModel model);
        Task<ResponseData<object>> Update(AppUserSaveModel model);
        Task<ResponseData<object>> ResetPassword(ResetPasswordModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
