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
    /// Repository quản lý câu hỏi trong phiên thi (ExamSessionQuestion), lưu thứ tự và điểm từng câu khi sinh đề.
    /// Kết hợp Dapper cho danh sách admin và EF Core cho tra cứu câu hỏi theo phiên thi.
    /// </summary>
    public class ExamSessionQuestionRepository : RepositoryBaseAsync<ExamSessionQuestion, long, DataContext>, IExamSessionQuestionRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public ExamSessionQuestionRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Danh sách câu hỏi trong phiên thi có phân trang — JOIN để hiển thị nội dung câu và thông tin session.
        /// </summary>
        public async Task<IEnumerable<ExamSessionQuestionModel>> GetListPaging(ExamSessionQuestionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@"
                SELECT esq.Id, esq.ExamSessionId, esq.QuestionId, esq.Points, esq.SortOrder,
                       q.Content, q.QuestionType,
                       CONCAT('Session #', es.Id, ' - ', u.FullName) AS ExamSessionInfo
                FROM ExamSessionQuestions esq
                INNER JOIN ExamSessions es ON esq.ExamSessionId = es.Id
                INNER JOIN Users u ON es.UserId = u.Id
                INNER JOIN Questions q ON esq.QuestionId = q.Id ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo phiên thi cụ thể
            if (search.ExamSessionId > 0)
            {
                query.Append(" AND esq.ExamSessionId = @ExamSessionId");
                dynamicParameters.Add("ExamSessionId", search.ExamSessionId);
            }
            // Lọc theo câu hỏi gốc trong ngân hàng
            if (search.QuestionId > 0)
            {
                query.Append(" AND esq.QuestionId = @QuestionId");
                dynamicParameters.Add("QuestionId", search.QuestionId);
            }
            // Tìm theo nội dung câu hỏi
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND q.Content LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            query.Append(" ORDER BY esq.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            return await DapperQueryAsync<ExamSessionQuestionModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Đếm tổng câu hỏi trong phiên theo bộ lọc — dùng cho phân trang.
        /// </summary>
        public async Task<long> GetTotalRecord(ExamSessionQuestionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@"
                SELECT COUNT(1)
                FROM ExamSessionQuestions esq
                INNER JOIN Questions q ON esq.QuestionId = q.Id
                WHERE 1 = 1");
            if (search.ExamSessionId > 0)
            {
                query.Append(" AND esq.ExamSessionId = @ExamSessionId");
                dynamicParameters.Add("ExamSessionId", search.ExamSessionId);
            }
            if (search.QuestionId > 0)
            {
                query.Append(" AND esq.QuestionId = @QuestionId");
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
        /// Lấy câu hỏi của phiên thi theo SortOrder — đảm bảo thí sinh thấy đúng thứ tự đề đã sinh.
        /// </summary>
        public async Task<IList<ExamSessionQuestion>> GetByExamSessionId(long examSessionId)
        {
            return await FindByCondition(x => x.ExamSessionId == examSessionId)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }
    }
}
