using Microsoft.EntityFrameworkCore;
using Example.Common.Base;
using Example.Common.Repository.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using Dapper;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Common.Repository
{
    /// <summary>
    /// Lớp cơ sở Repository generic cho Entity Framework: truy vấn LINQ, raw SQL/Dapper, giao dịch và CRUD.
    /// Các Repository cụ thể (User, ExamSet...) kế thừa và bổ sung truy vấn nghiệp vụ riêng.
    /// </summary>
#pragma warning disable S2436, S2931
    public class RepositoryBaseAsync<T, K, TContext> : IRepositoryBaseAsync<T, K, TContext> where T : EntityBase<K> where TContext : DbContext
#pragma warning restore S2436, S2931
    {
        // DbContext EF Core — nguồn truy vấn và theo dõi thay đổi entity.
        protected readonly TContext _dbContext;
        // Unit of Work — gom SaveChanges và quản lý transaction.
        protected readonly IUnitOfWork<TContext> _unitOfWork;

        // Constructor mặc định (dùng khi lớp con tự khởi tạo context/unitOfWork).
        public RepositoryBaseAsync()
        {

        }

        // Tiêm DbContext trực tiếp; tự tạo UnitOfWork mặc định (pattern consumer/test).
        public RepositoryBaseAsync(TContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _unitOfWork = new UnitOfWork<TContext>(_dbContext);
        }

        // Tiêm cả DbContext và UnitOfWork (DI đầy đủ, dùng chung transaction giữa nhiều repo).
        public RepositoryBaseAsync(TContext dbContext, IUnitOfWork<TContext> unitOfWork)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        // cách này khả năng ko tự động đóng kết nối db
        //public RepositoryBaseAsync(string connectionString)
        //{
        //    if (!string.IsNullOrEmpty(connectionString))
        //    {
        //        var options = new DbContextOptionsBuilder<TContext>().UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)).Options;
        //        _dbContext = Activator.CreateInstance(typeof(TContext), options) as TContext;
        //        _unitOfWork = new UnitOfWork<TContext>(_dbContext);
        //    }
        //}

        // Khởi tạo qua IServiceProvider + connection string — API layer dùng Scoped DbContext từ DI container.
        public RepositoryBaseAsync(IServiceProvider serviceProvider, string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                var options = new DbContextOptionsBuilder<TContext>().UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)).Options;
                _dbContext = ActivatorUtilities.CreateInstance<TContext>(serviceProvider, options);
                _unitOfWork = ActivatorUtilities.CreateInstance<UnitOfWork<TContext>>(serviceProvider, _dbContext);
            }
        }

        // --- Nhóm truy vấn LINQ: đọc toàn bộ hoặc theo điều kiện, có/không tracking và Include ---

        /// <summary>Lấy IQueryable toàn bộ bảng; trackChanges=false dùng AsNoTracking (chỉ đọc).</summary>
        public IQueryable<T> FindAll(bool trackChanges = false)
        {
            return !trackChanges ? _dbContext.Set<T>().AsNoTracking() : _dbContext.Set<T>();
        }

        /// <summary>FindAll kèm eager-load các navigation property (Include).</summary>
        public IQueryable<T> FindAll(bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties)
        {
            var items = FindAll(trackChanges);
            items = includeProperties.Aggregate(items, (current, includeProperty) => current.Include(includeProperty));
            return items;
        }

        /// <summary>Lọc entity theo biểu thức lambda; hỗ trợ AsNoTracking khi chỉ đọc.</summary>
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false)
        {
            return !trackChanges
            ? _dbContext.Set<T>().Where(expression).AsNoTracking()
            : _dbContext.Set<T>().Where(expression);
        }

        // --- Nhóm raw SQL / Dapper: truy vấn phức tạp, đếm bản ghi, thực thi lệnh ngoài LINQ ---

        /// <summary>Truy vấn entity bằng SQL tham số hóa (FromSqlRaw) thay vì LINQ.</summary>
        public IQueryable<T> FindByConditionWithParam(string query, IList<MySqlParameter> parameters)
        {
            return _dbContext.Set<T>().FromSqlRaw(query, parameters.ToArray());
        }

        /// <summary>Đếm tổng bản ghi qua câu SQL trả về int (phân trang, báo cáo).</summary>
        public IQueryable<int> CountTotalRecordWithParam(string query, IList<MySqlParameter> parameters)
        {
            return _dbContext.Database.SqlQueryRaw<int>(query, parameters.ToArray());
        }

        /// <summary>Dapper: truy vấn nhiều dòng với DynamicParameters.</summary>
        public async Task<IEnumerable<T>> DapperQueryAsync<T>(string query, DynamicParameters dynamicParameters)
        {
            var connection = _dbContext.Database.GetDbConnection();
            IEnumerable<T> result = await connection.QueryAsync<T>(query, dynamicParameters);
            return result;
        }

        /// <summary>Dapper: lấy một bản ghi (hoặc null) với DynamicParameters.</summary>
        public async Task<T> DapperGetAsync<T>(string query, DynamicParameters parms)
        {
            var connection = _dbContext.Database.GetDbConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(query, parms);
        }

        /// <summary>Dapper: lấy một bản ghi trong phạm vi transaction hiện có.</summary>
        public async Task<T> DapperGetAsync<T>(string query, DynamicParameters parms,IDbTransaction transaction)
        {
            var connection = _dbContext.Database.GetDbConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(query, parms,transaction);
        }

        /// <summary>Dapper: thực thi INSERT/UPDATE/DELETE, trả số dòng bị ảnh hưởng.</summary>
        public async Task<int> DapperExecuteAsync(string query, DynamicParameters parms)
        {
            var connection = _dbContext.Database.GetDbConnection();
            return await connection.ExecuteAsync(query, parms);
        }

        /// <summary>Dapper: thực thi truy vấn trả về một giá trị đơn (COUNT, MAX...).</summary>
        public async Task<T> DapperExecuteScalarAsync<T>(string query, DynamicParameters parms)
        {
            var connection = _dbContext.Database.GetDbConnection();
            return await connection.ExecuteScalarAsync<T>(query, parms);
        }

        /// <summary>Thực thi câu SQL thuần không tham số qua EF (DDL, TRUNCATE tùy biến...).</summary>
        public async Task ExecuteSqlRawAsync(string sql)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(sql);
        }

        /// <summary>Lọc theo điều kiện kèm eager-load navigation properties.</summary>
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties)
        {
            var items = FindByCondition(expression, trackChanges);
            items = includeProperties.Aggregate(items, (current, includeProperty) => current.Include(includeProperty));
            return items;
        }

        /// <summary>Lấy entity theo khóa chính Id (không Include).</summary>
        public async Task<T?> GetByIdAsync(K id) => await FindByCondition(x => x.Id.Equals(id))
        .FirstOrDefaultAsync();

        /// <summary>Lấy entity theo Id kèm các navigation property chỉ định.</summary>
        public async Task<T?> GetByIdAsync(K id, params Expression<Func<T, object>>[] includeProperties) =>
        await FindByCondition(x => x.Id.Equals(id), trackChanges: false, includeProperties)
        .FirstOrDefaultAsync();

        // --- Nhóm giao dịch: ủy quyền Begin/Commit/Rollback sang UnitOfWork ---

        //public Task<IDbContextTransaction> BeginTransactionAsync() => _dbContext.Database.BeginTransactionAsync();
        //public async Task<int> EndTransactionAsync()
        //{
        //    int result = await SaveChangesAsync();
        //    await _dbContext.Database.CommitTransactionAsync();
        //    return result;
        //}
        //public Task RollbackTransactionAsync() => _dbContext.Database.RollbackTransactionAsync();

        /// <summary>Bắt đầu transaction EF; các thao tác sau dùng chung một DbContext.</summary>
        public Task<IDbContextTransaction> BeginTransactionAsync() => _unitOfWork.BeginTransactionAsync();

        /// <summary>Lưu thay đổi và commit transaction (EndTransaction = Commit + SaveChanges).</summary>
        public async Task<int> EndTransactionAsync() => await _unitOfWork.CommitTransactionAsync();

        /// <summary>Hủy transaction và giải phóng tài nguyên.</summary>
        public Task RollbackTransactionAsync() => _unitOfWork.RollbackTransactionAsync();

        // --- Nhóm CRUD: thêm/sửa/xóa entity qua DbSet (chưa persist cho đến SaveChanges) ---

        /// <summary>Thêm một entity mới vào context; trả Id sau khi gán (cần SaveChanges để ghi DB).</summary>
        public async Task<K> CreateAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity.Id;
        }

        /// <summary>Thêm hàng loạt entity; trả danh sách Id tương ứng.</summary>
        public async Task<IList<K>> CreateListAsync(IEnumerable<T> entities)
        {
            await _dbContext.Set<T>().AddRangeAsync(entities);
            return entities.Select(x => x.Id).ToList();
        }

        /// <summary>Cập nhật entity: load bản ghi hiện có rồi SetValues (tránh attach trùng).</summary>
        public Task UpdateAsync(T entity)
        {
            if (_dbContext.Entry(entity).State == EntityState.Unchanged)
                return Task.CompletedTask;
            T exist = _dbContext.Set<T>().Find(entity.Id);
            _dbContext.Entry(exist).CurrentValues.SetValues(entity);
            return Task.CompletedTask;
        }

        /// <summary>Cập nhật nhiều entity; bỏ qua bản ghi không đổi (Unchanged).</summary>
        public Task UpdateListAsync(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                if (_dbContext.Entry(entity).State == EntityState.Unchanged)
                    continue;
                T exist = _dbContext.Set<T>().Find(entity.Id);
                _dbContext.Entry(exist).CurrentValues.SetValues(entity);
            }
            return Task.CompletedTask;
        }

        /// <summary>Xóa một entity khỏi DbSet (cần SaveChanges để xóa trên DB).</summary>
        public Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }

        /// <summary>Xóa hàng loạt entity qua RemoveRange.</summary>
        public Task DeleteListAsync(IEnumerable<T> entities)
        {
            _dbContext.Set<T>().RemoveRange(entities);
            return Task.CompletedTask;
        }

        // --- Nhóm persist: ghi mọi thay đổi đang track trong context xuống database ---

        /// <summary>Lưu thay đổi (INSERT/UPDATE/DELETE) chưa commit; trả số bản ghi bị ảnh hưởng.</summary>
        public Task<int> SaveChangesAsync() => _unitOfWork.SaveChangesAsync();

        // --- Nhóm Dapper (object param): overload đơn giản không dùng DynamicParameters ---

        /// <summary>Dapper: lấy một bản ghi với tham số anonymous/object.</summary>
        public async Task<T> DapperGetAsync<T>(string query, object? param)
        {
            var connection = _dbContext.Database.GetDbConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(query, param);
        }

        /// <summary>Dapper: truy vấn nhiều dòng với tham số anonymous/object.</summary>
        public async Task<IEnumerable<T>> DapperQueryAsync<T>(string query, object? param)
        {
            var connection = _dbContext.Database.GetDbConnection();
            return await connection.QueryAsync<T>(query, param);
        }

        /// <summary>Dapper: thực thi lệnh ghi với tham số anonymous/object.</summary>
        public async Task<int> DapperExecuteAsync(string query, object? param)
        {
            var connection = _dbContext.Database.GetDbConnection();
            return await connection.ExecuteAsync(query, param);
        }

        // --- Nhóm tiện ích bảo trì ---

        /// <summary>Xóa toàn bộ dữ liệu bảng (TRUNCATE) — chỉ dùng khi chắc chắn, không rollback soft.</summary>
        public async Task TruncateTable(string tableName)
        {
            string sql = $"TRUNCATE TABLE {tableName};";
            await _dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
