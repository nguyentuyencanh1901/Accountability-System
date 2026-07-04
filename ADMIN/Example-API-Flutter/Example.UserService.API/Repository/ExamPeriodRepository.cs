using Dapper;
using Example.Common.Enums;
using Example.Common.Repository;
using Example.Common.Repository.Interfaces;
using Example.Common.Utilities;
using Example.UserService.API.DBContexts;
using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Example.UserService.API.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Example.UserService.API.Repository
{
    /// <summary>
    /// Repository quản lý kỳ thi (ExamPeriod), bao gồm kiểm tra trùng tên, lọc kỳ hết hạn và phân trang.
    /// Kết hợp EF Core cho job đóng kỳ tự động và Dapper cho danh sách admin có subquery đếm phân công.
    /// </summary>
    public class ExamPeriodRepository : RepositoryBaseAsync<ExamPeriod, long, DataContext>, IExamPeriodRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public ExamPeriodRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Kiểm tra tên kỳ thi trùng — đảm bảo mỗi kỳ có tên duy nhất trong hệ thống.
        /// </summary>
        public Task<bool> CheckNameExists(string name, long id)
        {
            return FindByCondition(x => x.Name == name && x.Id != id).AnyAsync();
        }

        /// <summary>
        /// Lấy các kỳ đã Publish nhưng đã quá EndAt — dùng cho job tự động đóng kỳ thi.
        /// </summary>
        public Task<List<ExamPeriod>> GetPublishedExpiredAsync(DateTimeOffset now)
        {
            return FindByCondition(x =>
                    x.Status == (int)ExamPeriodStatusEnum.Published &&
                    x.EndAt < now)
                .ToListAsync();
        }

        /// <summary>
        /// Danh sách kỳ thi có phân trang kèm số lượng phân công — subquery COUNT tránh N+1 query.
        /// </summary>
        public async Task<IEnumerable<ExamPeriodModel>> GetListPaging(ExamPeriodSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@"
                SELECT ep.Id, ep.Name, ep.Description, ep.StartAt, ep.EndAt, ep.Status,
                       (SELECT COUNT(1) FROM ExamPeriodAssignments epa WHERE epa.ExamPeriodId = ep.Id) AS AssignmentCount
                FROM ExamPeriods ep ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo id kỳ thi cụ thể
            if (search.Id > 0)
            {
                query.Append(" AND ep.Id = @Id");
                dynamicParameters.Add("Id", search.Id);
            }
            // Lọc theo trạng thái kỳ (nháp/publish/đóng...)
            if (search.Status > 0)
            {
                query.Append(" AND ep.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            // Tìm theo tên kỳ thi
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND ep.Name LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            query.Append(" ORDER BY ep.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            return await DapperQueryAsync<ExamPeriodModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Đếm tổng kỳ thi khớp bộ lọc — đồng bộ với GetListPaging để tính phân trang chính xác.
        /// </summary>
        public async Task<long> GetTotalRecord(ExamPeriodSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT COUNT(1) FROM ExamPeriods ep ");
            query.Append(" WHERE 1 = 1");
            if (search.Id > 0)
            {
                query.Append(" AND ep.Id = @Id");
                dynamicParameters.Add("Id", search.Id);
            }
            if (search.Status > 0)
            {
                query.Append(" AND ep.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND ep.Name LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            return await DapperGetAsync<int>(query.ToString(), dynamicParameters);
        }
    }
}
