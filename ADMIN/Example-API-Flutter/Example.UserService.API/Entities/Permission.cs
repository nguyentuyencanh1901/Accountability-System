using Example.Common.Base;

namespace Example.UserService.API.Entities
{

    public class Permission : EntityAuditBase<long>
    {
        public long Id { get; set; }
        public string Code { get; set; }  // USER_CREATE
        public string Description { get; set; }
    }

}
