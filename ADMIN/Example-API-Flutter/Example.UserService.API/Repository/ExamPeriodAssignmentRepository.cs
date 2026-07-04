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
    /// Repository quản lý phân công thí sinh vào kỳ thi (ExamPeriodAssignment), bao gồm tra cứu và phân trang.
    /// Kết hợp EF Core cho kiểm tra trùng/phân công active và Dapper cho danh sách admin có JOIN đa bảng.
    /// </summary>
    public class ExamPeriodAssignmentRepository : RepositoryBaseAsync<ExamPeriodAssignment, long, DataContext>, IExamPeriodAssignmentRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public ExamPeriodAssignmentRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Kiểm tra đã tồn tại phân công cùng kỳ + user + loại thi — tránh gán trùng một thí sinh hai lần cùng loại.
        /// </summary>
        public Task<bool> CheckAssignmentExists(long examPeriodId, long userId, int examType, long id)
        {
            // Loại trừ bản ghi đang sửa (id) để không tự báo trùng chính mình
            return FindByCondition(x =>
                    x.ExamPeriodId == examPeriodId &&
                    x.UserId == userId &&
                    x.ExamType == examType &&
                    x.Id != id)
                .AnyAsync();
        }

        /// <summary>
        /// Lấy danh sách UserId duy nhất đã được phân vào kỳ — dùng khi gán hàng loạt hoặc thống kê.
        /// </summary>
        public Task<List<long>> GetUserIdsByPeriodIdAsync(long examPeriodId)
        {
            return FindByCondition(x => x.ExamPeriodId == examPeriodId)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// Lấy toàn bộ phân công thô theo kỳ thi — phục vụ xử lý nghiệp vụ nội bộ (cập nhật trạng thái hàng loạt).
        /// </summary>
        public Task<List<ExamPeriodAssignment>> GetByPeriodIdAsync(long examPeriodId)
        {
            return FindByCondition(x => x.ExamPeriodId == examPeriodId).ToListAsync();
        }

        /// <summary>
        /// Đếm số phân công trong kỳ — hiển thị thống kê trên màn hình quản lý kỳ thi.
        /// </summary>
        public Task<long> CountByExamPeriodId(long examPeriodId)
        {
            return FindByCondition(x => x.ExamPeriodId == examPeriodId).LongCountAsync();
        }

        /// <summary>
        /// Lấy phân công theo id và user — đảm bảo thí sinh chỉ truy cập phân công của chính mình.
        /// </summary>
        public Task<ExamPeriodAssignment?> GetUserAssignmentAsync(long userId, long assignmentId)
        {
            return FindByCondition(x => x.Id == assignmentId && x.UserId == userId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tìm phân công đang hoạt động (chưa hoàn thành) theo user + bộ đề + loại thi — tránh tạo session trùng.
        /// </summary>
        public Task<ExamPeriodAssignment?> GetActiveByUserExamSetTypeAsync(long userId, long examSetId, int examType)
        {
            return FindByCondition(x =>
                    x.UserId == userId &&
                    x.ExamSetId == examSetId &&
                    x.ExamType == examType &&
                    x.Status != (int)ExamPeriodAssignmentStatusEnum.Completed
                    && x.Status != (int)ExamPeriodAssignmentStatusEnum.Cancelled)
                // Lấy bản ghi mới nhất nếu có nhiều — ưu tiên assignment gần đây nhất
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy danh sách phân công của thí sinh đang đăng nhập — chỉ hiện kỳ đã Publish hoặc Closed.
        /// </summary>
        public async Task<IEnumerable<ExamPeriodAssignmentModel>> GetMyAssignmentsAsync(long userId)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            // JOIN để lấy tên kỳ, tên thí sinh, tên bộ đề; COALESCE xử lý trường hợp bộ đề ngẫu nhiên (ExamSetId null)
            var query = @"
                SELECT epa.Id, epa.ExamPeriodId, epa.UserId, epa.ExamSetId, epa.ExamType, epa.Status, epa.ExamSessionId,
                       ep.Name AS ExamPeriodName, u.FullName AS UserFullName,
                       COALESCE(es.Name, N'Bộ đề ngẫu nhiên') AS ExamSetName,
                       ep.Status AS ExamPeriodStatus, ep.StartAt AS ExamPeriodStartAt, ep.EndAt AS ExamPeriodEndAt
                FROM ExamPeriodAssignments epa
                INNER JOIN ExamPeriods ep ON epa.ExamPeriodId = ep.Id
                INNER JOIN Users u ON epa.UserId = u.Id
                LEFT JOIN ExamSets es ON epa.ExamSetId = es.Id
                WHERE epa.UserId = @UserId AND ep.Status IN (@PublishedStatus, @ClosedStatus)
                ORDER BY ep.StartAt DESC, epa.Id DESC";
            dynamicParameters.Add("UserId", userId);
            // Chỉ lấy kỳ đã công bố hoặc đã đóng — ẩn kỳ nháp/chưa publish khỏi thí sinh
            dynamicParameters.Add("PublishedStatus", (int)ExamPeriodStatusEnum.Published);
            dynamicParameters.Add("ClosedStatus", (int)ExamPeriodStatusEnum.Closed);
            return await DapperQueryAsync<ExamPeriodAssignmentModel>(query, dynamicParameters);
        }

        /// <summary>
        /// Danh sách phân công có phân trang cho admin — dùng Dapper kết hợp bộ lọc động.
        /// </summary>
        public async Task<IEnumerable<ExamPeriodAssignmentModel>> GetListPaging(ExamPeriodAssignmentSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(BuildAssignmentSelectQuery());
            query.Append(" WHERE 1 = 1");
            AppendSearchFilters(query, dynamicParameters, search);
            query.Append(" ORDER BY epa.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            return await DapperQueryAsync<ExamPeriodAssignmentModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Lấy phân công theo kỳ kèm thông tin session — dùng màn hình chi tiết kỳ thi (danh sách thí sinh + điểm).
        /// </summary>
        public async Task<IEnumerable<ExamPeriodAssignmentModel>> GetByPeriodIdWithSessionAsync(long examPeriodId)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(BuildAssignmentSelectQuery());
            query.Append(" WHERE epa.ExamPeriodId = @ExamPeriodId");
            // Sắp theo tên thí sinh để dễ đọc trên báo cáo
            query.Append(" ORDER BY u.FullName, epa.ExamType, epa.Id");
            dynamicParameters.Add("ExamPeriodId", examPeriodId);
            return await DapperQueryAsync<ExamPeriodAssignmentModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Truy vấn SELECT chung — tái sử dụng để đồng nhất cột giữa phân trang và tra cứu theo kỳ.
        /// </summary>
        private static string BuildAssignmentSelectQuery() => @"
                SELECT epa.Id, epa.ExamPeriodId, epa.UserId, epa.ExamSetId, epa.ExamType, epa.Status, epa.ExamSessionId,
                       ep.Name AS ExamPeriodName, u.FullName AS UserFullName,
                       COALESCE(es.Name, sessEs.Name, N'Bộ đề ngẫu nhiên') AS ExamSetName,
                       sess.TotalScore AS SessionTotalScore, sess.MaxScore AS SessionMaxScore,
                       sess.StartedAt AS SessionStartedAt, sess.FinishedAt AS SessionFinishedAt,
                       sess.Status AS SessionStatus
                FROM ExamPeriodAssignments epa
                INNER JOIN ExamPeriods ep ON epa.ExamPeriodId = ep.Id
                INNER JOIN Users u ON epa.UserId = u.Id
                LEFT JOIN ExamSets es ON epa.ExamSetId = es.Id
                LEFT JOIN ExamSessions sess ON sess.Id = epa.ExamSessionId
                LEFT JOIN ExamSets sessEs ON sess.ExamSetId = sessEs.Id ";

        /// <summary>
        /// Đếm tổng bản ghi phân công theo bộ lọc — phục vụ phân trang UI.
        /// </summary>
        public async Task<long> GetTotalRecord(ExamPeriodAssignmentSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@"
                SELECT COUNT(1)
                FROM ExamPeriodAssignments epa
                INNER JOIN ExamPeriods ep ON epa.ExamPeriodId = ep.Id
                INNER JOIN Users u ON epa.UserId = u.Id
                LEFT JOIN ExamSets es ON epa.ExamSetId = es.Id ");
            query.Append(" WHERE 1 = 1");
            AppendSearchFilters(query, dynamicParameters, search);
            return await DapperGetAsync<int>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Ghép điều kiện lọc động vào câu SQL — mỗi filter chỉ áp dụng khi giá trị tìm kiếm có ý nghĩa (>0 hoặc không rỗng).
        /// </summary>
        private static void AppendSearchFilters(
            StringBuilder query,
            DynamicParameters dynamicParameters,
            ExamPeriodAssignmentSearchModel search)
        {
            if (search.Id > 0)
            {
                query.Append(" AND epa.Id = @Id");
                dynamicParameters.Add("Id", search.Id);
            }
            if (search.Status > 0)
            {
                query.Append(" AND epa.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            if (search.ExamPeriodId > 0)
            {
                query.Append(" AND epa.ExamPeriodId = @ExamPeriodId");
                dynamicParameters.Add("ExamPeriodId", search.ExamPeriodId);
            }
            if (search.UserId > 0)
            {
                query.Append(" AND epa.UserId = @UserId");
                dynamicParameters.Add("UserId", search.UserId);
            }
            if (search.ExamSetId > 0)
            {
                query.Append(" AND epa.ExamSetId = @ExamSetId");
                dynamicParameters.Add("ExamSetId", search.ExamSetId);
            }
            if (search.ExamType > 0)
            {
                query.Append(" AND epa.ExamType = @ExamType");
                dynamicParameters.Add("ExamType", search.ExamType);
            }
            // Tìm kiếm mờ theo tên thí sinh, tên bộ đề hoặc tên kỳ — LIKE %keyword% hỗ trợ tìm một phần
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND (u.FullName LIKE @Keyword OR es.Name LIKE @Keyword OR ep.Name LIKE @Keyword)");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
        }
    }
}
