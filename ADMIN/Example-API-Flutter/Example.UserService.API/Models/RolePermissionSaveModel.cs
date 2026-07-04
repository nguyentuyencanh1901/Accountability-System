using Example.Common.Models;
using Example.UserService.API.Entities;

namespace Example.UserService.API.Models
{
    public class RolePermissionModel : RolePermissionSaveModel
    {
    }
    public class RolePermissionSaveModel
    {
        public long Id { get; set; }
        public long RoleId { get; set; }
        public long PermissionId { get; set; }
    }

    public class RolePermissionSearchModel : BaseSearch
    {
    }
}
