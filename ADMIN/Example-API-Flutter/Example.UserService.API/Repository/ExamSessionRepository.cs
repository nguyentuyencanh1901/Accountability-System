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
    /// Repository quản lý phiên thi (ExamSession), bao gồm phân trang, lịch sử thi và tra cứu phiên đang làm.
    /// Kết hợp Dapper cho báo cáo phức tạp và EF Core cho tra cứu phiên InProgress theo user/bộ đề.
    /// </summary>
    public class ExamSessionRepository : RepositoryBaseAsync<ExamSession, long, DataContext>, IExamSessionRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public ExamSessionRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Danh sách phiên thi có phân trang kèm tên bộ đề và thí sinh — JOIN để hiển thị đầy đủ trên admin.
        /// </summary>
        public async Task<IEnumerable<ExamSessionModel>> GetListPaging(ExamSessionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@"
                SELECT es.Id, es.ExamSetId, es.UserId, es.ExamType, es.StartedAt, es.FinishedAt,
                       es.TotalScore, es.MaxScore, es.Status, es.ViolationCount,
                       e.Name AS ExamSetName, u.FullName AS UserFullName
                FROM ExamSessions es
                INNER JOIN ExamSets e ON es.ExamSetId = e.Id
                INNER JOIN Users u ON es.UserId = u.Id ");
            query.Append(" WHERE 1 = 1");
            // HistoryOnly: chỉ lấy phiên đã nộp/hoàn thành (status 2,3) — phân biệt màn hình lịch sử vs phiên đang thi
            if (search.HistoryOnly)
            {
                query.Append(" AND es.Status IN (2, 3, 4)");
            }
            else if (search.Status > 0)
            {
                query.Append(" AND es.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            // Lọc theo bộ đề cụ thể
            if (search.ExamSetId > 0)
            {
                query.Append(" AND es.ExamSetId = @ExamSetId");
                dynamicParameters.Add("ExamSetId", search.ExamSetId);
            }
            // Lọc theo thí sinh
            if (search.UserId > 0)
            {
                query.Append(" AND es.UserId = @UserId");
                dynamicParameters.Add("UserId", search.UserId);
            }
            // Lọc theo loại thi (luyện tập/chính thức...)
            if (search.ExamType > 0)
            {
                query.Append(" AND es.ExamType = @ExamType");
                dynamicParameters.Add("ExamType", search.ExamType);
            }
            query.Append(" ORDER BY es.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            return await DapperQueryAsync<ExamSessionModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Đếm tổng phiên thi — dùng cùng bộ lọc với GetListPaging để tính phân trang chính xác.
        /// </summary>
        public async Task<long> GetTotalRecord(ExamSessionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT COUNT(1) FROM ExamSessions es ");
            query.Append(" WHERE 1 = 1");
            if (search.HistoryOnly)
            {
                query.Append(" AND es.Status IN (2, 3, 4)");
            }
            else if (search.Status > 0)
            {
                query.Append(" AND es.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            if (search.ExamSetId > 0)
            {
                query.Append(" AND es.ExamSetId = @ExamSetId");
                dynamicParameters.Add("ExamSetId", search.ExamSetId);
            }
            if (search.UserId > 0)
            {
                query.Append(" AND es.UserId = @UserId");
                dynamicParameters.Add("UserId", search.UserId);
            }
            if (search.ExamType > 0)
            {
                query.Append(" AND es.ExamType = @ExamType");
                dynamicParameters.Add("ExamType", search.ExamType);
            }
            return await DapperGetAsync<int>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Đếm phiên thi của một user — tái sử dụng GetTotalRecord để đồng nhất logic lọc.
        /// </summary>
        public Task<long> CountByUserId(long userId)
            => GetTotalRecord(new ExamSessionSearchModel { UserId = userId, PageIndex = 1, PageSize = 1 });

        /// <summary>
        /// Tìm phiên đang làm (InProgress) theo user + bộ đề + loại thi — ngăn mở nhiều phiên song song.
        /// </summary>
        public async Task<ExamSession?> GetInProgressAsync(long userId, long examSetId, int examType)
        {
            return await FindByCondition(x =>
                    x.UserId == userId &&
                    x.ExamSetId == examSetId &&
                    x.ExamType == examType &&
                    x.Status == (int)ExamSessionStatusEnum.InProgress)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tìm phiên InProgress theo assignment — liên kết trực tiếp với phân công kỳ thi.
        /// </summary>
        public async Task<ExamSession?> GetInProgressByAssignmentAsync(long assignmentId)
        {
            return await FindByCondition(x =>
                    x.ExamPeriodAssignmentId == assignmentId &&
                    x.Status == (int)ExamSessionStatusEnum.InProgress)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy tất cả phiên đang làm của user — dùng kiểm tra trước khi cho phép bắt đầu thi mới.
        /// </summary>
        public async Task<List<ExamSession>> GetInProgressByUserAsync(long userId)
        {
            return await FindByCondition(x =>
                    x.UserId == userId &&
                    x.Status == (int)ExamSessionStatusEnum.InProgress)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Danh sách thí sinh đã làm bài theo bộ đề kèm thống kê đúng/sai/bỏ qua — dùng màn hình báo cáo bộ đề.
        /// </summary>
        public async Task<IEnumerable<ExamSetTakerModel>> GetTakersByExamSet(ExamSetTakerSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("ExamSetId", search.ExamSetId);

            // Subquery tq: đếm tổng câu hỏi trong phiên; subquery ast: tổng hợp đúng/sai/bỏ qua từ ExamSessionAnswers
            var query = new StringBuilder(@"
                SELECT
                    es.Id AS ExamSessionId,
                    es.UserId,
                    u.FullName AS UserFullName,
                    u.Username,
                    es.ExamType,
                    es.StartedAt,
                    es.FinishedAt,
                    es.TotalScore,
                    es.MaxScore,
                    es.Status,
                    IFNULL(tq.TotalQuestions, 0) AS TotalQuestions,
                    IFNULL(ast.CorrectCount, 0) AS CorrectCount,
                    IFNULL(ast.WrongCount, 0) AS WrongCount,
                    IFNULL(ast.SkippedCount, 0) AS SkippedCount
                FROM ExamSessions es
                INNER JOIN Users u ON es.UserId = u.Id
                LEFT JOIN (
                    SELECT ExamSessionId, COUNT(*) AS TotalQuestions
                    FROM ExamSessionQuestions
                    GROUP BY ExamSessionId
                ) tq ON tq.ExamSessionId = es.Id
                LEFT JOIN (
                    SELECT
                        esa.ExamSessionId,
                        SUM(CASE WHEN esa.IsCorrect = 1 THEN 1 ELSE 0 END) AS CorrectCount,
                        SUM(CASE WHEN esa.IsCorrect = 0 AND EXISTS (
                            SELECT 1 FROM ExamSessionAnswerOptions esao
                            WHERE esao.ExamSessionAnswerId = esa.Id LIMIT 1
                        ) THEN 1 ELSE 0 END) AS WrongCount,
                        SUM(CASE WHEN esa.IsCorrect = 0 AND NOT EXISTS (
                            SELECT 1 FROM ExamSessionAnswerOptions esao
                            WHERE esao.ExamSessionAnswerId = esa.Id LIMIT 1
                        ) THEN 1 ELSE 0 END) AS SkippedCount
                    FROM ExamSessionAnswers esa
                    GROUP BY esa.ExamSessionId
                ) ast ON ast.ExamSessionId = es.Id
                WHERE es.ExamSetId = @ExamSetId
                ORDER BY es.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            return await DapperQueryAsync<ExamSetTakerModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Đếm tổng thí sinh đã có phiên thi theo bộ đề — phục vụ phân trang danh sách GetTakersByExamSet.
        /// </summary>
        public async Task<long> GetTakersTotalByExamSet(ExamSetTakerSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("ExamSetId", search.ExamSetId);
            const string query = @"SELECT COUNT(1) FROM ExamSessions es WHERE es.ExamSetId = @ExamSetId";
            return await DapperGetAsync<int>(query, dynamicParameters);
        }
    }
}

