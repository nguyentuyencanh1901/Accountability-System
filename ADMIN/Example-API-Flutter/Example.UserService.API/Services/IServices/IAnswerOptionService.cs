using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IAnswerOptionService
    {
        Task<ResponseData<IEnumerable<AnswerOptionModel>>> GetListPaging(AnswerOptionSearchModel search);
        Task<ResponseData<AnswerOptionModel>> GetById(long id);
        Task<ResponseData<object>> Insert(AnswerOptionSaveModel model);
        Task<ResponseData<object>> Update(AnswerOptionSaveModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
