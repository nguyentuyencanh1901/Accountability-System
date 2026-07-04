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
    /// Repository quản lý câu hỏi (Question) trong ngân hàng câu hỏi, hỗ trợ phân trang và lọc câu hỏi đang hoạt động.
    /// Kết hợp Dapper cho danh sách có bộ lọc và EF Core cho tra cứu câu hỏi active theo lĩnh vực.
    /// </summary>
    public class QuestionRepository : RepositoryBaseAsync<Question, long, DataContext>, IQuestionRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public QuestionRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Danh sách câu hỏi có phân trang — Dapper cho hiệu năng khi lọc nhiều tiêu chí, JOIN Fields lấy tên lĩnh vực.
        /// </summary>
        public async Task<IEnumerable<QuestionModel>> GetListPaging(QuestionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT d.Id, d.FieldId, f.Name AS FieldName, d.Content, d.ImageUrl, d.Points, d.QuestionType, d.DifficultyLevel, d.Status FROM Questions d LEFT JOIN Fields f ON d.FieldId = f.Id ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo lĩnh vực câu hỏi
            if (search.FieldId > 0)
            {
                query.Append(" AND d.FieldId = @FieldId");
                dynamicParameters.Add("FieldId", search.FieldId);
            }
            // Lọc theo trạng thái — 0 nghĩa là lấy tất cả
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            // Lọc theo loại câu hỏi (trắc nghiệm, tự luận...)
            if (search.QuestionType > 0)
            {
                query.Append(" AND d.QuestionType = @QuestionType");
                dynamicParameters.Add("QuestionType", search.QuestionType);
            }
            // Lọc theo mức độ khó — phục vụ sinh đề theo cấu hình Easy/Medium/Hard
            if (search.DifficultyLevel > 0)
            {
                query.Append(" AND d.DifficultyLevel = @DifficultyLevel");
                dynamicParameters.Add("DifficultyLevel", search.DifficultyLevel);
            }
            // Tìm theo nội dung câu hỏi — hỗ trợ tìm kiếm nhanh trên UI
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND d.Content LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            query.Append(" ORDER BY d.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            return await DapperQueryAsync<QuestionModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Đếm tổng câu hỏi theo bộ lọc — dùng cho phân trang, đồng bộ điều kiện với GetListPaging.
        /// </summary>
        public async Task<long> GetTotalRecord(QuestionSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT COUNT(1) FROM Questions d ");
            query.Append(" WHERE 1 = 1");
            if (search.FieldId > 0)
            {
                query.Append(" AND d.FieldId = @FieldId");
                dynamicParameters.Add("FieldId", search.FieldId);
            }
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            if (search.QuestionType > 0)
            {
                query.Append(" AND d.QuestionType = @QuestionType");
                dynamicParameters.Add("QuestionType", search.QuestionType);
            }
            if (search.DifficultyLevel > 0)
            {
                query.Append(" AND d.DifficultyLevel = @DifficultyLevel");
                dynamicParameters.Add("DifficultyLevel", search.DifficultyLevel);
            }
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND d.Content LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            return await DapperGetAsync<int>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Lấy tất cả câu hỏi đang hoạt động (Status=1) — dùng khi sinh đề hoặc thống kê ngân hàng câu hỏi.
        /// </summary>
        public Task<List<Question>> GetActiveQuestionsAsync()
        {
            return FindByCondition(x => x.Status == 1).ToListAsync();
        }

        /// <summary>
        /// Lấy câu hỏi active theo danh sách lĩnh vực — dùng khi sinh đề ngẫu nhiên chỉ từ các Field được cấu hình trong bộ đề.
        /// </summary>
        public Task<List<Question>> GetActiveQuestionsByFieldIdsAsync(IEnumerable<long> fieldIds)
        {
            // Loại bỏ id không hợp lệ và trùng lặp — tránh query rỗng hoặc điều kiện IN thừa
            var ids = fieldIds.Where(x => x > 0).Distinct().ToList();
            if (ids.Count == 0)
                return Task.FromResult(new List<Question>());

            return FindByCondition(x => x.Status == 1 && ids.Contains(x.FieldId)).ToListAsync();
        }
    }
}
