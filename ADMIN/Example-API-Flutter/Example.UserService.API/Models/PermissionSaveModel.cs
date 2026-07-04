using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class PermissionModel : PermissionSaveModel
    {
    }
    public class PermissionSaveModel
    {
        public long Id { get; set; }
        public string Code { get; set; }  // USER_CREATE
        public string Description { get; set; }

    }

    public class PermissionSearchModel : BaseSearch
    {
    }
}
