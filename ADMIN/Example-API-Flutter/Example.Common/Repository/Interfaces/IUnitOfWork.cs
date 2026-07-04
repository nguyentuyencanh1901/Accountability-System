using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Repository.Interfaces
{
    /// <summary>
    /// Hợp đồng Unit of Work: lưu thay đổi và điều phối transaction cho một DbContext.
    /// </summary>
    public interface IUnitOfWork<TContext> : IDisposable where TContext : DbContext
    {
        // --- Nhóm persist ---
        Task<int> SaveChangesAsync();

        // --- Nhóm transaction ---
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
