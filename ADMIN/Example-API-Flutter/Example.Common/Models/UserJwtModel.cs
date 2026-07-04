using Example.Common.Utilities;
using System.Text.Json.Serialization;

namespace Example.Common.Models
{
    public class UserJwtModel
    {
        public long UserId { get; set; }
        public int UserType { get; set; }
        public long CustomerId { get; set; }
        public virtual string UserName { get; set; }
        public virtual string FullName { get; set; }
        public virtual string Email { get; set; }
        public string Roles { get; set; }
        public string DeviceId { get; set; }
    }

    public class UserRoleDataJwtDTO
    {
        public long RoleId { get; set; }

        [JsonIgnore]
        public string Ids { get; set; }
        public List<long> ListId { get => this.Ids.ToListT(long.Parse); }
    }

    public class RoleDataJwtDTO
    {
        public long RoleId { get; set; }
        public List<long> ListId { get; set; }
    }
}
