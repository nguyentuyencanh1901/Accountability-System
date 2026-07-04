using Example.Common.Base;

namespace Example.Common.Entities
{
    public class Notification : EntityAuditBase<long>
    {
        public long CustomerId { get; set; }
        public long DeviceId { get; set; }
        public long CameraId { get; set; }
        public int PackageNameType { get; set; }
        public long UserViolate { get; set; }
        public long NotifyTime { get; set; }
        public int NotifyType { get; set; }
        public int MediaType { get; set; }
        public string BucketMedia { get; set; }
        public string ObjectMedia { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        //public int TimeWarningDuration { get; set; }
        public string EvidenceKey { get; set; }
    }
}
