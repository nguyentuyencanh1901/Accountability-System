using Example.Common.Enums;
using System.Security.Principal;

namespace Example.Common.Models
{
    public class NotificationUserModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long NotificationId { get; set; }
        public bool MarkRead { get; set; }
        public bool MarkSeen { get; set; }
        public long CustomerId { get; set; }
        public long CameraId { get; set; }
        public long DeviceId { get; set; }
        public int PackageNameType { get; set; }
        public long UserViolate { get; set; }
        public long NotifyTime { get; set; }
        public NotifyType NotifyType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public NotifyMediaType MediaType { get; set; }
        public string BucketMedia { get; set; }
        public string ObjectMedia { get; set; }
        public string UrlMedia { get; set; }
        public string DeviceName { get; set; }
        public DateTime NotifyDate { get; set; }
        public string EvidenceKey { get; set; }
    }

    public class NotificationUserSearchModel : BaseSearch
    {
        public long AreaId { get; set; }
        public long DeviceId { get; set; }
        public int PackageNameType { get; set; }
        public long UserId { get; set; }
        public int NotifyType { get; set; }
        public bool? MarkRead { get; set; }
        public bool? MarkSeen { get; set; }
    }
}
