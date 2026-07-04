using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IQuestionService
    {
        Task<ResponseData<IEnumerable<QuestionModel>>> GetListPaging(QuestionSearchModel search);
        Task<ResponseData<QuestionModel>> GetById(long id);
        Task<ResponseData<object>> Insert(QuestionSaveModel model);
        Task<ResponseData<object>> Update(QuestionSaveModel model);
        Task<ResponseData<object>> UpdateStatus(UpdateStatusModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
