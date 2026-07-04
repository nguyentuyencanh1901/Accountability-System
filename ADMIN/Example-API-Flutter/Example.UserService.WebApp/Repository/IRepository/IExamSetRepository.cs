using Example.Common.Models;
using Example.UserService.API.Models;

namespace Example.UserService.WebApp.Repository.IRepository
{
    public interface IExamSetRepository
    {
        Task<ResponseData<ExamSetModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    }
}
