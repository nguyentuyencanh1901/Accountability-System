using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebApp.Repository.IRepository
{
    public interface IAppUserRepository
    {
        Task<ResponseData<AppUserModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    }
}
