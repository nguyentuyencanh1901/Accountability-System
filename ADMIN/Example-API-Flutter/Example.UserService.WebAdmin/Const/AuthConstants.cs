using Example.Common.Enums;

namespace Example.UserService.WebAdmin.Const
{
    public static class AuthConstants
    {
        /// <summary>Quyền Super Admin — bypass mọi permission (giống API)</summary>
        public static string SuperAdminPermission => PermissionCodeEnum.SUPER_ADMIN.GetCode();

        /// <summary>Tên vai trò Super Admin — chỉ seed 1 tài khoản, không gán qua form.</summary>
        public const string SuperAdminRoleName = "SuperAdmin";

        public static bool IsSuperAdminRoleName(string? roleName) =>
            string.Equals(roleName, SuperAdminRoleName, StringComparison.OrdinalIgnoreCase);
    }
}
