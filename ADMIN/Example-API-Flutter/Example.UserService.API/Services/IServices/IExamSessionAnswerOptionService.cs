using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IExamSessionAnswerOptionService
    {
        Task<ResponseData<IEnumerable<ExamSessionAnswerOptionModel>>> GetListPaging(ExamSessionAnswerOptionSearchModel search);
        Task<ResponseData<ExamSessionAnswerOptionModel>> GetById(long id);
        Task<ResponseData<object>> Insert(ExamSessionAnswerOptionSaveModel model);
        Task<ResponseData<object>> Update(ExamSessionAnswerOptionSaveModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
