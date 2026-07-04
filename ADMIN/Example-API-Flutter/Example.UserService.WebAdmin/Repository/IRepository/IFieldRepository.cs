using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebAdmin.Repository.IRepository
{
    public interface IFieldRepository
    {
        Task<ResponseData<List<FieldModel>>> GetListAsync(FieldSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<FieldModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(FieldSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(FieldSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateStatusAsync(UpdateStatusModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
