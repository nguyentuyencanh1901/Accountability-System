using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IFieldService
    {
        Task<ResponseData<IEnumerable<FieldModel>>> GetListPaging(FieldSearchModel search);
        Task<ResponseData<FieldModel>> GetById(long id);
        Task<ResponseData<object>> Insert(FieldSaveModel model);
        Task<ResponseData<object>> Update(FieldSaveModel model);
        Task<ResponseData<object>> UpdateStatus(UpdateStatusModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
