using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IExamSessionAnswerService
    {
        Task<ResponseData<IEnumerable<ExamSessionAnswerModel>>> GetListPaging(ExamSessionAnswerSearchModel search);
        Task<ResponseData<ExamSessionAnswerModel>> GetById(long id);
        Task<ResponseData<object>> Insert(ExamSessionAnswerSaveModel model);
        Task<ResponseData<object>> Update(ExamSessionAnswerSaveModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
