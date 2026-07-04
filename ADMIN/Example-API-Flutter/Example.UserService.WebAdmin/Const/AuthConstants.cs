using Example.Common.Enums;

namespace Example.UserService.WebAdmin.Const
{
    public static class AuthConstants
    {
        /// <summary>Quyền Super Admin — bypass mọi permission (giống API)</summary>
        public static string SuperAdminPermission => PermissionCodeEnum.SUPER_ADMIN.GetCode();
    }
}
