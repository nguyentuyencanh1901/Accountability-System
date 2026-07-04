using Example.Common.Cache;
using Example.Common.Utilities.Helper;
using Example.UserService.API.Entities;
using Example.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Example.UserService.API.DBContexts
{
    public static class PermissionSeedExtensions
    {
        public static async Task SeedPermissionsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            var redisCache = scope.ServiceProvider.GetService<IRedisCache>();

            var defined = PermissionCodeExtensions.GetAllWithDescriptions()
                .ToDictionary(x => x.Code, x => x.Description, StringComparer.OrdinalIgnoreCase);

            var existing = await db.Permissions.AsNoTracking().ToListAsync();
            var changed = false;

            foreach (var (code, description) in defined)
            {
                var row = existing.FirstOrDefault(p =>
                    string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase));

                if (row == null)
                {
                    db.Permissions.Add(new Permission { Code = code, Description = description });
                    changed = true;
                    continue;
                }

                if (!string.Equals(row.Description, description, StringComparison.Ordinal))
                {
                    var tracked = await db.Permissions.FirstAsync(p => p.Id == row.Id);
                    tracked.Description = description;
                    changed = true;
                }
            }

            var obsolete = existing
                .Where(p => !defined.ContainsKey(p.Code))
                .ToList();

            foreach (var row in obsolete)
            {
                db.RolePermissions.RemoveRange(
                    await db.RolePermissions.Where(rp => rp.PermissionId == row.Id).ToListAsync());
                var tracked = await db.Permissions.FirstAsync(p => p.Id == row.Id);
                db.Permissions.Remove(tracked);
                changed = true;
            }

            if (!changed)
                return;

            await db.SaveChangesAsync();
            await InvalidatePermissionCacheAsync(redisCache);
            Log.Information("Đã đồng bộ {Count} quyền hạn từ PermissionCodeEnum vào database.", defined.Count);
        }

        private static async Task InvalidatePermissionCacheAsync(IRedisCache? redisCache)
        {
            if (redisCache == null)
                return;

            var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", "Permission_GetListPaging");
            await redisCache.RemoveCacheStartWithAsync(cacheKey);
        }
    }
}
