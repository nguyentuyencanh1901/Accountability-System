using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class UserRole : EntityAuditBase<long>
    {
        public long UserId { get; set; }
        public AppUser User { get; set; }

        public long RoleId { get; set; }
        public Role Role { get; set; }
    }

}
