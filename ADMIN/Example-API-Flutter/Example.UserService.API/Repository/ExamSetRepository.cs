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
    /// Repository quản lý bộ đề thi (ExamSet), bao gồm kiểm tra trùng tên và phân trang.
    /// Kết hợp EF Core cho tra cứu đơn giản và Dapper cho truy vấn danh sách có bộ lọc động.
    /// </summary>
    public class ExamSetRepository : RepositoryBaseAsync<ExamSet, long, DataContext>, IExamSetRepository
    {
        /// <summary>Khởi tạo repository với DbContext và UnitOfWork từ lớp cơ sở.</summary>
        public ExamSetRepository(DataContext dbContext, IUnitOfWork<DataContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Kiểm tra tên bộ đề trùng — tránh nhầm lẫn khi admin chọn hoặc tạo bộ đề mới.
        /// </summary>
        public Task<bool> CheckNameExists(string name, long id)
        {
            return FindByCondition(x => x.Name == name && x.Id != id).AnyAsync();
        }

        /// <summary>
        /// Danh sách bộ đề có phân trang — Dapper cho hiệu năng khi lọc theo trạng thái, loại và từ khóa.
        /// </summary>
        public async Task<IEnumerable<ExamSetModel>> GetListPaging(ExamSetSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT d.Id, d.Name, d.Description, d.RequiredTotalPoints, d.QuestionCount, d.EasyCount, d.MediumCount, d.HardCount, d.Type, d.DurationMinutes, d.Status FROM ExamSets d ");
            query.Append(" WHERE 1 = 1");
            // Lọc theo trạng thái hoạt động — 0 nghĩa là lấy tất cả
            if (search.Status > 0)
            {
                query.Append(" AND d.Status = @Status");
                dynamicParameters.Add("Status", search.Status);
            }
            // Tìm theo tên bộ đề — hỗ trợ tìm kiếm nhanh trên UI
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                query.Append(" AND d.Name LIKE @Keyword");
                dynamicParameters.Add("Keyword", $"%{search.Keyword}%");
            }
            // Lọc theo loại bộ đề (cố định/ngẫu nhiên...)
            if (search.Type > 0)
            {
                query.Append(" AND d.Type = @Type");
                dynamicParameters.Add("Type", search.Type);
            }
            query.Append(" ORDER BY d.Id DESC ");
            StringUtils.AddPaging(query, search.PageIndex, search.PageSize);
            return await DapperQueryAsync<ExamSetModel>(query.ToString(), dynamicParameters);
        }

        /// <summary>
        /// Đếm tổng bộ đề theo bộ lọc — dùng cho phân trang, đồng bộ điều kiện với GetListPaging.
        /// </summary>
        public async Task<long> GetTotalRecord(ExamSetSearchModel search)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            StringBuilder query = new StringBuilder(@" SELECT COUNT(1) FROM ExamSets d ");
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
            if (search.Type > 0)
            {
                query.Append(" AND d.Type = @Type");
                dynamicParameters.Add("Type", search.Type);
            }
            return await DapperGetAsync<int>(query.ToString(), dynamicParameters);
        }
    }
}
