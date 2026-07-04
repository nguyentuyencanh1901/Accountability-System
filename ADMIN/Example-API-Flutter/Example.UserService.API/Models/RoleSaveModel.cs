using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class RoleModel : RoleSaveModel
    {
        public int PermissionCount { get; set; }
    }
    public class RoleSaveModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public List<long> Permissions { get; set; }
    }

    public class RoleSearchModel : BaseSearch
    {
    }
}
