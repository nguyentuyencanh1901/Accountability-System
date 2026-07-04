using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Example.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using Dapper;

namespace Example.Common.Repository.Interfaces
{
#pragma warning disable S2436 // Types and methods should not have too many generic parameters
    /// <summary>
    /// Hợp đồng truy vấn đọc: LINQ, raw SQL, Dapper và GetById — không ghi dữ liệu.
    /// </summary>
    public interface IRepositoryQueryBase<T, K, TContext> where T : EntityBase<K> where TContext : DbContext
    {
        // --- Nhóm truy vấn LINQ ---
        IQueryable<T> FindAll(bool trackChanges);
        IQueryable<T> FindAll(bool trackChanges, params Expression<Func<T, object>>[] includeProperties);
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges);
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges, params Expression<Func<T, object>>[] includeProperties);

        // --- Nhóm raw SQL / Dapper (DynamicParameters) ---
        IQueryable<T> FindByConditionWithParam(string query, IList<MySqlParameter> parameters);
        IQueryable<int> CountTotalRecordWithParam(string query, IList<MySqlParameter> parameters);
        Task<IEnumerable<T>> DapperQueryAsync<T>(string query, DynamicParameters dynamicParameters);

        // --- Nhóm đọc theo khóa ---
        Task<T?> GetByIdAsync(K id);
        Task<T?> GetByIdAsync(K id, params Expression<Func<T, object>>[] includeProperties);

        // --- Nhóm Dapper (object param) ---
        Task<IEnumerable<T>> DapperQueryAsync<T>(string query, object? param);
        Task<T> DapperGetAsync<T>(string query, object? param);
        Task<int> DapperExecuteAsync(string query, object? param);
    }

    /// <summary>
    /// Hợp đồng Repository đầy đủ: kế thừa truy vấn + CRUD, transaction và tiện ích bảo trì.
    /// </summary>
    public interface IRepositoryBaseAsync<T, K, TContext> : IRepositoryQueryBase<T, K, TContext> where T : EntityBase<K> where TContext : DbContext
    {
        // --- Nhóm CRUD ---
        Task<K> CreateAsync(T entity);
        Task<IList<K>> CreateListAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateListAsync(IEnumerable<T> entities);
        Task DeleteAsync(T entity);
        Task DeleteListAsync(IEnumerable<T> entities);

        // --- Nhóm tiện ích bảo trì ---
        Task TruncateTable(string tableName);

        // --- Nhóm persist và transaction ---
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> EndTransactionAsync();
        Task RollbackTransactionAsync();
    }

#pragma warning restore S2436 // Types and methods should not have too many generic parameters
}
