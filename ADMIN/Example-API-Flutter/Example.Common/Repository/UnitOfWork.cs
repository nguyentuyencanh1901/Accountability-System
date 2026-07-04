using Microsoft.EntityFrameworkCore;
using Example.Common.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.Common.Repository
{
    /// <summary>
    /// Triển khai Unit of Work cho một DbContext: gom SaveChanges và quản lý transaction EF Core.
    /// Repository gọi qua interface để nhiều repo cùng chia sẻ một transaction khi cần.
    /// </summary>
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        // DbContext được inject — không tự tạo để đồng bộ với DI Scoped.
        private readonly TContext _context;
        // Transaction hiện tại (null khi chưa Begin hoặc đã Commit/Rollback).
        private IDbContextTransaction _transaction;

        public UnitOfWork(TContext context)
        {
            _context = context;
        }

        // --- Nhóm persist: ghi thay đổi không bọc transaction ---

        /// <summary>Lưu mọi thay đổi đang track trong context; không tự commit transaction.</summary>
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        // --- Nhóm transaction: Begin → thao tác qua repo → Commit hoặc Rollback ---

        /// <summary>Mở transaction mới; lưu reference để Commit/Rollback sau.</summary>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        /// <summary>SaveChanges rồi commit transaction; giải phóng _transaction.</summary>
        public async Task<int> CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                int result = await SaveChangesAsync();
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
                return result;
            }
            return 0;
        }

        /// <summary>Hủy transaction và giải phóng tài nguyên; không gọi SaveChanges.</summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // --- Nhóm vòng đời: giải phóng transaction và DbContext ---

        /// <summary>Dispose transaction (nếu còn) và DbContext khi hết scope DI.</summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
