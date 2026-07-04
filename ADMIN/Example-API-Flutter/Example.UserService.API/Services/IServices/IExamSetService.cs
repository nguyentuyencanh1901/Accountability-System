using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.API.Services.IServices
{
    public interface IExamSetService
    {
        Task<ResponseData<IEnumerable<ExamSetModel>>> GetListPaging(ExamSetSearchModel search);
        Task<ResponseData<ExamSetModel>> GetById(long id);
        Task<ResponseData<object>> Insert(ExamSetSaveModel model);
        Task<ResponseData<object>> Update(ExamSetSaveModel model);
        Task<ResponseData<object>> Delete(long id);
        Task<ResponseData<IEnumerable<ExamSetTakerModel>>> GetExamTakers(ExamSetTakerSearchModel search);
    }
}
