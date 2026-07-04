using Example.Common.Base.Interfaces;
using Example.Common.Const;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Example.Common.Base
{
    public class DbConsumerContextBase<TContext> : DbContext where TContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DbConsumerContextBase()
        {
        }

        public DbConsumerContextBase(DbContextOptions<TContext> options) : base(options)
        {
        }

        public DbConsumerContextBase(DbContextOptions<TContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
        {
            var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "Consumer";
            var modified = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted);
            foreach (var item in modified)
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        if (item.Entity is IDateTracking addedEntity)
                        {
                            addedEntity.CreatedDate = DateTimeOffset.Now;
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
                            modifiedEntity.LastModifiedDate = DateTimeOffset.Now;
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
            //return base.SaveChangesAsync(cancellationToken);

            // Save changes to the database
            var result = base.SaveChangesAsync(cancellationToken);

            // Detach entities after saving changes
            foreach (var entry in ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }

            return result;
        }
    }
}
