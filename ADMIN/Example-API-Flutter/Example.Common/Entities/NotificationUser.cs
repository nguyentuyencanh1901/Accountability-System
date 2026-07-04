using Example.Common.Base;

namespace Example.Common.Entities
{
    public class NotificationUser : EntityAuditBase<long>
    {
        public long UserId { get; set; }
        public long NotificationId { get; set; }
        public bool MarkRead { get; set; }
        public bool MarkSeen { get; set; }
    }
}
