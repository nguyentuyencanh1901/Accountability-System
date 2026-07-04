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
    /// Repository quản lý các đáp án đã chọn trong câu trả lời phiên thi (ExamSessionAnswerOption).
    /// Kết hợp Dapper cho danh sách admin và EF Core cho tra cứu/chấm điểm hàng loạt.
    /// </summary>
    public class ExamSessionAnswerOptionRepository : RepositoryBaseAsync<ExamSessionAnswerOption, long, DataContext>, IExamSessionAnswerOptionRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public ExamSessionAnswerOptionRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Danh sách đáp án đã chọn có phân trang — JOIN để hiển thị nội dung đáp án và câu hỏi liên quan.
        /// </summary>
        public async Task<IEnumerable<ExamSessionAnswerOptionModel>> GetListPaging(ExamSessionAnswerOptionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            // LEFT cắt ngắn nội dung dài để tránh payload quá lớn trên danh sách admin
            StringBuilder query = new StringBuilder(@"
                SELECT esao.Id, esao.ExamSessionAnswerId, esao.AnswerOptionId,
                       LEFT(ao.Content, 100) AS AnswerOptionContent,
                       CONCAT('Answer #', esa.Id, ' - ', LEFT(q.Content, 50)) AS ExamSessionAnswerInfo
                FROM ExamSessionAnswerOptions esao
                INNER JOIN ExamSessionAnswers esa ON esao.ExamSessionAnswerId = esa.Id
                INNER JOIN AnswerOptions ao ON esao.AnswerOptionId = ao.Id
                INNER JOIN Questions q ON esa.QuestionId = q.Id ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo câu trả lời phiên thi cụ thể
            if (search.ExamSessionAnswerId > 0)
            {
                query.Append(" AND esao.ExamSessionAnswerId = @ExamSessionAnswerId");
                dynamicParameters.Add("ExamSessionAnswerId", search.ExamSessionAnswerId);
            }
            // Lọc theo đáp án gốc trong ngân hàng
            if (search.AnswerOptionId > 0)
            {
                query.Append(" AND esao.AnswerOptionId = @AnswerOptionId");
                dynamicParameters.Add("AnswerOptionId", search.AnswerOptionId);
            }
            // Tìm theo nội dung đáp án
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND ao.Content LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            query.Append(" ORDER BY esao.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            return await DapperQueryAsync<ExamSessionAnswerOptionModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Đếm tổng bản ghi — COUNT nhẹ hơn SELECT khi chỉ cần số trang.
        /// </summary>
        public async Task<long> GetTotalRecord(ExamSessionAnswerOptionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@"
                SELECT COUNT(1)
                FROM ExamSessionAnswerOptions esao
                INNER JOIN AnswerOptions ao ON esao.AnswerOptionId = ao.Id
                WHERE 1 = 1");
            if (search.ExamSessionAnswerId > 0)
            {
                query.Append(" AND esao.ExamSessionAnswerId = @ExamSessionAnswerId");
                dynamicParameters.Add("ExamSessionAnswerId", search.ExamSessionAnswerId);
            }
            if (search.AnswerOptionId > 0)
            {
                query.Append(" AND esao.AnswerOptionId = @AnswerOptionId");
                dynamicParameters.Add("AnswerOptionId", search.AnswerOptionId);
            }
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND ao.Content LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            return await DapperGetAsync<int>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Lấy hàng loạt đáp án đã chọn theo danh sách ExamSessionAnswerId — tối ưu khi chấm điểm nhiều câu cùng lúc.
        /// </summary>
        public async Task<IList<ExamSessionAnswerOption>> GetByExamSessionAnswerIds(List<long> examSessionAnswerIds)
        {
            return await FindByCondition(x => examSessionAnswerIds.Contains(x.ExamSessionAnswerId)).ToListAsync();
        }

        /// <summary>
        /// Đếm số lần một đáp án được chọn — hỗ trợ kiểm tra trước khi xóa AnswerOption khỏi ngân hàng.
        /// </summary>
        public async Task<int> CountByAnswerOptionId(long answerOptionId)
        {
            return await FindByCondition(x => x.AnswerOptionId == answerOptionId).CountAsync();
        }
    }
}
