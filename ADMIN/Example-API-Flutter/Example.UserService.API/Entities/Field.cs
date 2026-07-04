using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class Field : EntityAuditBase<long>
    {
        public string Name { get; set; }
        public int Status { get; set; }
    }
}
