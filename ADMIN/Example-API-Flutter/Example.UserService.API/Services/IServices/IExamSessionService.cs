using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IExamSessionService
    {
        Task<ResponseData<IEnumerable<ExamSessionModel>>> GetListPaging(ExamSessionSearchModel search);
        Task<ResponseData<ExamSessionModel>> GetById(long id);
        Task<ResponseData<object>> StartExam(StartExamModel model);
        Task<ResponseData<object>> SaveExamProgress(SubmitExamModel model);
        Task<ResponseData<object>> SubmitExam(SubmitExamModel model);
        Task<ResponseData<object>> CancelExamDueToViolation(SubmitExamModel model);
        Task<ResponseData<object>> Insert(ExamSessionSaveModel model);
        Task<ResponseData<object>> Update(ExamSessionSaveModel model);
        Task<ResponseData<object>> Delete(long id);
    }
}
