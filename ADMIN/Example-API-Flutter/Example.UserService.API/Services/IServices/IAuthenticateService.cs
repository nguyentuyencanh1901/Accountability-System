using Example.Common.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IAuthenticateService
    {
        public Task<ResponseData<object>> Register(RegisterModel model);
        public Task<ResponseData<object>> Login(LoginModel model);
        public Task<ResponseData<object>> LoginApp(LoginModel model);
        Task<ResponseData<object>> UpdateProfile(long userId, UpdateProfileModel model);
        Task<ResponseData<object>> ChangePassword(long userId, ChangePasswordModel model);
    }
}
