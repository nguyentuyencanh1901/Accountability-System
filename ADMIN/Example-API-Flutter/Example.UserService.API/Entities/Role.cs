using Example.Common.Base;

namespace Example.UserService.API.Entities
{

    public class Role : EntityAuditBase<long>
    {
        public string Name { get; set; }
    }

}
