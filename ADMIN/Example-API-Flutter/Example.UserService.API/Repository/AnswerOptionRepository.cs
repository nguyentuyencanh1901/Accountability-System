using Example.Common.Repository;
using Example.Common.Repository.Interfaces;
using Example.Common.Utilities;
using Example.UserService.API.DBContexts;
using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Example.UserService.API.Repository.IRepository;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Example.UserService.API.Repository
{
    /// <summary>
    /// Repository quản lý đáp án (AnswerOption) của câu hỏi, hỗ trợ phân trang và tra cứu theo câu hỏi.
    /// Kết hợp Dapper cho danh sách admin và EF Core cho tra cứu đáp án theo QuestionId.
    /// </summary>
    public class AnswerOptionRepository : RepositoryBaseAsync<AnswerOption, long, DataContext>, IAnswerOptionRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public AnswerOptionRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Danh sách đáp án có phân trang — JOIN câu hỏi để hiển thị ngữ cảnh trên màn hình quản trị.
        /// </summary>
        public async Task<IEnumerable<AnswerOptionModel>> GetListPaging(AnswerOptionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@"
                SELECT ao.Id, ao.QuestionId, ao.Content, ao.IsCorrect, ao.SortOrder,
                       LEFT(q.Content, 100) AS QuestionContent
                FROM AnswerOptions ao
                INNER JOIN Questions q ON ao.QuestionId = q.Id ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo câu hỏi cụ thể
            if (search.QuestionId > 0)
            {
                query.Append(" AND ao.QuestionId = @QuestionId");
                dynamicParameters.Add("QuestionId", search.QuestionId);
            }
            // Tìm theo nội dung đáp án
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND ao.Content LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            query.Append(" ORDER BY ao.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            return await DapperQueryAsync<AnswerOptionModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Đếm tổng đáp án theo bộ lọc — dùng cho phân trang.
        /// </summary>
        public async Task<long> GetTotalRecord(AnswerOptionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT COUNT(1) FROM AnswerOptions ao WHERE 1 = 1");
            if (search.QuestionId > 0)
            {
                query.Append(" AND ao.QuestionId = @QuestionId");
                dynamicParameters.Add("QuestionId", search.QuestionId);
            }
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND ao.Content LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            return await DapperGetAsync<int>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Lấy đáp án của một câu hỏi theo SortOrder — thứ tự hiển thị trên UI thi.
        /// </summary>
        public async Task<IList<AnswerOption>> GetByQuestionId(long questionId)
        {
            return await FindByCondition(x => x.QuestionId == questionId)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy đáp án của nhiều câu hỏi cùng lúc — tránh N+1 query khi render đề thi nhiều câu.
        /// </summary>
        public async Task<IList<AnswerOption>> GetByQuestionIds(List<long> questionIds)
        {
            return await FindByCondition(x => questionIds.Contains(x.QuestionId))
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }
    }
}
