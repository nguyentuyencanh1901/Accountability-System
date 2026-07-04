using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebAdmin.Repository.IRepository
{
    public interface IAppUserRepository
    {
        Task<ResponseData<List<AppUserModel>>> GetListAsync(AppUserSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<AppUserModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(AppUserSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(AppUserSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> ResetPasswordAsync(ResetPasswordModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
