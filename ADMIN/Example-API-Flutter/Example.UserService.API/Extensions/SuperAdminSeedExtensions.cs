using Example.Common.Enums;
using Example.UserService.API.DBContexts;
using Example.UserService.API.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Example.UserService.API.Extensions
{
    public static class SuperAdminSeedExtensions
    {
        public static string SuperAdminPermissionCode => PermissionCodeEnum.SUPER_ADMIN.GetCode();
        public const string SuperAdminRoleName = "SuperAdmin";

        public static async Task SeedSuperAdminAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();

            var username = configuration["DefaultSuperAdmin:Username"] ?? "superadmin";
            var password = configuration["DefaultSuperAdmin:Password"] ?? "123456";
            var email = configuration["DefaultSuperAdmin:Email"] ?? "superadmin@local.dev";
            var fullName = configuration["DefaultSuperAdmin:FullName"] ?? "Super Admin";
            var phone = configuration["DefaultSuperAdmin:Phone"] ?? "00000000000";

            if (await db.Users.AnyAsync(u => u.Username == username))
                return;

            var permission = await db.Permissions
                .FirstOrDefaultAsync(p => p.Code == SuperAdminPermissionCode);
            if (permission == null)
            {
                Log.Warning("Chưa tìm thấy quyền SUPER_ADMIN — chạy SeedPermissionsAsync trước SeedSuperAdminAsync.");
                return;
            }

            var role = await db.Roles
                .FirstOrDefaultAsync(r => r.Name == SuperAdminRoleName);
            if (role == null)
            {
                role = new Role { Name = SuperAdminRoleName };
                db.Roles.Add(role);
                await db.SaveChangesAsync();
            }

            var hasRolePermission = await db.RolePermissions
                .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
            if (!hasRolePermission)
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
                await db.SaveChangesAsync();
            }

            var user = new AppUser
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Email = email,
                Phone = phone,
                FullName = fullName,
                Status = (int)StatusEnum.Active,
                UserType = (int)UserTypeEnum.Manager
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
            await db.SaveChangesAsync();

            Log.Information("Đã tạo tài khoản SuperAdmin mặc định: {Username}", username);
        }
    }
}
