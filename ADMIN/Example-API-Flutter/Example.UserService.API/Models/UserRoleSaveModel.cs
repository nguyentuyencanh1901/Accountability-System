using Example.Common.Models;
using Example.UserService.API.Entities;

namespace Example.UserService.API.Models
{
    public class UserRoleModel : UserRoleSaveModel
    {
    }
    public class UserRoleSaveModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long RoleId { get; set; }
    }

    public class UserRoleSearchModel : BaseSearch
    {
    }
}
