using Dapper;
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
    /// Repository quản lý câu trả lời trong phiên thi (ExamSessionAnswer), bao gồm điểm và kết quả đúng/sai.
    /// Kết hợp Dapper cho danh sách admin và EF Core cho tra cứu câu trả lời theo phiên thi.
    /// </summary>
    public class ExamSessionAnswerRepository : RepositoryBaseAsync<ExamSessionAnswer, long, DataContext>, IExamSessionAnswerRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public ExamSessionAnswerRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Danh sách câu trả lời có phân trang — JOIN session, user, question để hiển thị ngữ cảnh trên admin.
        /// </summary>
        public async Task<IEnumerable<ExamSessionAnswerModel>> GetListPaging(ExamSessionAnswerSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@"
                SELECT esa.Id, esa.ExamSessionId, esa.QuestionId, esa.IsCorrect, esa.Score,
                       LEFT(q.Content, 100) AS QuestionContent,
                       CONCAT('Session #', es.Id, ' - ', u.FullName) AS ExamSessionInfo
                FROM ExamSessionAnswers esa
                INNER JOIN ExamSessions es ON esa.ExamSessionId = es.Id
                INNER JOIN Users u ON es.UserId = u.Id
                INNER JOIN Questions q ON esa.QuestionId = q.Id ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo phiên thi cụ thể
            if (search.ExamSessionId > 0)
            {
                query.Append(" AND esa.ExamSessionId = @ExamSessionId");
                dynamicParameters.Add("ExamSessionId", search.ExamSessionId);
            }
            // Lọc theo câu hỏi gốc
            if (search.QuestionId > 0)
            {
                query.Append(" AND esa.QuestionId = @QuestionId");
                dynamicParameters.Add("QuestionId", search.QuestionId);
            }
            // Tìm theo nội dung câu hỏi
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND q.Content LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            query.Append(" ORDER BY esa.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            return await DapperQueryAsync<ExamSessionAnswerModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Đếm tổng câu trả lời theo bộ lọc — phục vụ phân trang.
        /// </summary>
        public async Task<long> GetTotalRecord(ExamSessionAnswerSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@"
                SELECT COUNT(1)
                FROM ExamSessionAnswers esa
                INNER JOIN Questions q ON esa.QuestionId = q.Id
                WHERE 1 = 1");
            if (search.ExamSessionId > 0)
            {
                query.Append(" AND esa.ExamSessionId = @ExamSessionId");
                dynamicParameters.Add("ExamSessionId", search.ExamSessionId);
            }
            if (search.QuestionId > 0)
            {
                query.Append(" AND esa.QuestionId = @QuestionId");
                dynamicParameters.Add("QuestionId", search.QuestionId);
            }
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND q.Content LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            return await DapperGetAsync<int>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Lấy toàn bộ câu trả lời của một phiên thi — dùng khi chấm điểm, xem lại bài hoặc tính tổng điểm.
        /// </summary>
        public async Task<IList<ExamSessionAnswer>> GetByExamSessionId(long examSessionId)
        {
            return await FindByCondition(x => x.ExamSessionId == examSessionId).ToListAsync();
        }
    }
}
