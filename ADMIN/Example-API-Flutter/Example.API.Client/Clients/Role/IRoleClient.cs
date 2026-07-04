using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.API.Client.Clients.Role
{
    /// <summary>Client cho RoleController</summary>
    public interface IRoleClient
    {
        Task<ResponseData<List<RoleModel>>> GetListAsync(RoleSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<RoleModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(RoleSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(RoleSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
