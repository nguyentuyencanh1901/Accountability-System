using Example.Common.Base.Interfaces;
using Example.Common.Entities;
using Example.Common.Services.IServices;
using Example.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;
using System.Text.Json;

namespace Example.Common.Base
{
    public class DbContextBase<TContext> : DbContext where TContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserCommonService _userCommonService;

        public DbContextBase()
        {
        }

        public DbContextBase(DbContextOptions<TContext> options) : base(options)
        {
        }

        public DbContextBase(DbContextOptions<TContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbContextBase(DbContextOptions<TContext> options,
            IHttpContextAccessor httpContextAccessor,
            IUserCommonService userCommonService)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _userCommonService = userCommonService;
        }

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
        {
            bool isAuditLogs = false;
            var now = DateTimeOffset.Now;
            var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "Unknown";
            var modified = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted);
            foreach (var item in modified)
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        if (item.Entity is IDateTracking addedEntity)
                        {
                            addedEntity.CreatedDate = now;
                        }
                        if (item.Entity is IUserTracking userAddEntity)
                        {
                            userAddEntity.CreatedBy = currentUser;
                        }
                        break;

                    case EntityState.Modified:
                        Entry(item.Entity).Property("Id").IsModified = false;
                        if (item.Entity is IDateTracking modifiedEntity)
                        {
                            modifiedEntity.LastModifiedDate = now;
                        }
                        if (item.Entity is IUserTracking userEditEntity)
                        {
                            userEditEntity.LastModifiedBy = currentUser;
                        }
                        break;

                    case EntityState.Deleted:
                        break;

                    default:
                        break;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }

    }
}
