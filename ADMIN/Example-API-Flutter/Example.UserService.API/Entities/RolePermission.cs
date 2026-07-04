using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class RolePermission : EntityAuditBase<long>
    {
        public long RoleId { get; set; }
        public long PermissionId { get; set; }
    }

}
