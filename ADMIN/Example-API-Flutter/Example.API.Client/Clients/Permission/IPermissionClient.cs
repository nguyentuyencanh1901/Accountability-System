using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.API.Client.Clients.Permission
{
    /// <summary>Client cho PermissionController</summary>
    public interface IPermissionClient
    {
        Task<ResponseData<List<PermissionModel>>> GetListAsync(PermissionSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<PermissionModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(PermissionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(PermissionSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
