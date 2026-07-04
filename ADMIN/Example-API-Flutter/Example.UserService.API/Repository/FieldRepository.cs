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
    /// Repository quản lý lĩnh vực câu hỏi (Field), bao gồm kiểm tra trùng tên, đếm câu hỏi và phân trang.
    /// Kết hợp Dapper cho truy vấn danh sách và EF Core cho đếm/quan hệ với bảng Questions.
    /// </summary>
    public class FieldRepository : RepositoryBaseAsync<Field, long, DataContext>, IFieldRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public FieldRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Kiểm tra tên lĩnh vực trùng — loại trừ bản ghi đang sửa để tránh báo trùng chính mình khi cập nhật.
        /// </summary>
        public Task<bool> CheckNameExists(string name, long id)
        {
            return FindByCondition(x => x.Name == name && x.Id != id).AnyAsync();
        }

        /// <summary>
        /// Đếm số câu hỏi thuộc lĩnh vực — dùng cảnh báo trước khi xóa/vô hiệu hóa lĩnh vực còn câu hỏi.
        /// </summary>
        public Task<long> CountQuestionsByFieldIdAsync(long fieldId)
        {
            return _dbContext.Set<Question>().LongCountAsync(x => x.FieldId == fieldId);
        }

        /// <summary>
        /// Danh sách lĩnh vực có phân trang — Dapper cho hiệu năng khi lọc theo trạng thái và từ khóa.
        /// </summary>
        public async Task<IEnumerable<FieldModel>> GetListPaging(FieldSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT d.Id, d.Name, d.Status FROM Fields d ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo trạng thái — 0 nghĩa là lấy tất cả
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            // Tìm theo tên lĩnh vực — hỗ trợ tìm kiếm nhanh trên UI admin
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND d.Name LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            query.Append(" ORDER BY d.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            return await DapperQueryAsync<FieldModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Đếm tổng lĩnh vực theo bộ lọc — dùng cho phân trang, đồng bộ điều kiện với GetListPaging.
        /// </summary>
        public async Task<long> GetTotalRecord(FieldSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT COUNT(1) FROM Fields d ");
            query.Append(" WHERE 1 = 1");
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND d.Name LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            return await DapperGetAsync<int>(query.ToString(), dynamicParameters);
        }
    }
}
