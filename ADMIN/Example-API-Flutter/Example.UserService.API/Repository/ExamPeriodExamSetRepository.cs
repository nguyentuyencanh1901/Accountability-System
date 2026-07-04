using Example.Common.Repository;
using Example.Common.Repository.Interfaces;
using Example.UserService.API.DBContexts;
using Example.UserService.API.Entities;
using Example.UserService.API.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Example.UserService.API.Repository
{
    /// <summary>
    /// Repository quản lý liên kết giữa kỳ thi (ExamPeriod) và bộ đề (ExamSet).
    /// Sử dụng Entity Framework Core cho tra cứu danh sách bộ đề thuộc một kỳ thi.
    /// </summary>
    public class ExamPeriodExamSetRepository : RepositoryBaseAsync<ExamPeriodExamSet, long, DataContext>, IExamPeriodExamSetRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public ExamPeriodExamSetRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Lấy danh sách ExamSetId thuộc một kỳ thi — dùng khi gán đề ngẫu nhiên hoặc kiểm tra bộ đề hợp lệ.
        /// </summary>
        public Task<List<long>> GetExamSetIdsByPeriodIdAsync(long examPeriodId)
        {
            return FindByCondition(x => x.ExamPeriodId == examPeriodId)
                .Select(x => x.ExamSetId)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy toàn bộ bản ghi liên kết theo kỳ thi — cần khi cập nhật/xóa hàng loạt mapping.
        /// </summary>
        public Task<List<ExamPeriodExamSet>> GetByPeriodIdAsync(long examPeriodId)
        {
            return FindByCondition(x => x.ExamPeriodId == examPeriodId).ToListAsync();
        }
    }
}
