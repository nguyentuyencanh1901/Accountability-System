using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.API.Client.Clients.Field
{
    public interface IFieldClient
    {
        Task<ResponseData<List<FieldModel>>> GetListAsync(FieldSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<FieldModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(FieldSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(FieldSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateStatusAsync(UpdateStatusModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
