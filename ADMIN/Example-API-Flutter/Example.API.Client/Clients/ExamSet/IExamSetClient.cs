using Example.API.Client.Core;
using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.API.Client.Clients.ExamSet
{
    /// <summary>Client cho ExamSetController</summary>
    public interface IExamSetClient
    {
        Task<ResponseData<List<ExamSetModel>>> GetListAsync(ExamSetSearchModel search, CancellationToken cancellationToken = default);
        Task<ResponseData<ExamSetModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> AddAsync(ExamSetSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> UpdateAsync(ExamSetSaveModel model, CancellationToken cancellationToken = default);
        Task<ResponseData<object>> DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task<ResponseData<List<ExamSetTakerModel>>> GetExamTakersAsync(ExamSetTakerSearchModel search, CancellationToken cancellationToken = default);
    }
}
