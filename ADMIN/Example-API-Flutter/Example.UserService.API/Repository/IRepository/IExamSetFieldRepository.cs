using Example.UserService.API.Entities;

namespace Example.UserService.API.Repository.IRepository
{
    public interface IExamSetFieldRepository
    {
        Task<List<long>> GetFieldIdsByExamSetIdAsync(long examSetId);
        Task SyncFieldsAsync(long examSetId, IEnumerable<long> fieldIds);
        Task DeleteByExamSetIdAsync(long examSetId);
    }
}
