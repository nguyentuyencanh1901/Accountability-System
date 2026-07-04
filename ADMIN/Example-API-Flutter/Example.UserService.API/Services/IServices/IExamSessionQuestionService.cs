using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IExamSessionQuestionService
    {
        Task<ResponseData<IEnumerable<ExamSessionQuestionModel>>> GetListPaging(ExamSessionQuestionSearchModel search);
        Task<ResponseData<ExamSessionQuestionModel>> GetById(long id);
        Task<ResponseData<object>> Insert(ExamSessionQuestionSaveModel model);
        Task<ResponseData<object>> Update(ExamSessionQuestionSaveModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
