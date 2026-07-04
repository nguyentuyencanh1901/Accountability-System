using Example.Common.Repository;
using Example.Common.Repository.Interfaces;
using Example.UserService.API.DBContexts;
using Example.UserService.API.Entities;
using Example.UserService.API.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Example.UserService.API.Repository
{
    /// <summary>
    /// Repository quản lý liên kết bộ đề–lĩnh vực (ExamSetField), xác định các lĩnh vực câu hỏi được dùng khi sinh đề.
    /// Sử dụng Entity Framework Core cho tra cứu và đồng bộ danh sách lĩnh vực theo bộ đề.
    /// </summary>
    public class ExamSetFieldRepository : RepositoryBaseAsync<ExamSetField, long, DataContext>, IExamSetFieldRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public ExamSetFieldRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Lấy danh sách FieldId thuộc một bộ đề — dùng khi sinh đề ngẫu nhiên hoặc kiểm tra phạm vi lĩnh vực.
        /// </summary>
        public Task<List<long>> GetFieldIdsByExamSetIdAsync(long examSetId)
        {
            return FindByCondition(x => x.ExamSetId == examSetId)
                .Select(x => x.FieldId)
                .ToListAsync();
        }

        /// <summary>
        /// Đồng bộ danh sách lĩnh vực của bộ đề — thêm mới, xóa bỏ mapping không còn trong danh sách đích.
        /// Dùng khi admin cập nhật cấu hình lĩnh vực trên màn hình chỉnh sửa bộ đề.
        /// </summary>
        public async Task SyncFieldsAsync(long examSetId, IEnumerable<long> fieldIds)
        {
            // Loại bỏ id không hợp lệ (≤0) và trùng lặp — đảm bảo danh sách đích sạch trước khi so sánh
            var distinctIds = fieldIds.Where(x => x > 0).Distinct().ToList();
            // Lấy toàn bộ mapping hiện có của bộ đề từ DB
            var existing = await FindByCondition(x => x.ExamSetId == examSetId).ToListAsync();
            var existingIds = existing.Select(x => x.FieldId).ToHashSet();
            var targetIds = distinctIds.ToHashSet();

            // Xóa các lĩnh vực không còn trong danh sách mới — giữ DB khớp với cấu hình admin
            var toDelete = existing.Where(x => !targetIds.Contains(x.FieldId)).ToList();
            if (toDelete.Count > 0)
                await DeleteListAsync(toDelete);

            // Thêm mapping mới chưa tồn tại — chỉ insert những FieldId chưa được gán
            var toAdd = distinctIds.Where(id => !existingIds.Contains(id))
                .Select(id => new ExamSetField { ExamSetId = examSetId, FieldId = id });
            await CreateListAsync(toAdd);
        }

        /// <summary>
        /// Xóa toàn bộ liên kết lĩnh vực của một bộ đề — dùng khi xóa bộ đề hoặc reset cấu hình.
        /// </summary>
        public async Task DeleteByExamSetIdAsync(long examSetId)
        {
            var existing = await FindByCondition(x => x.ExamSetId == examSetId).ToListAsync();
            if (existing.Count > 0)
                await DeleteListAsync(existing);
        }
    }
}
