using Example.Common.Enums;

namespace Example.Common.Models
{
    public class NotificationModel
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public long DeviceId { get; set; }
        public long CameraId { get; set; }
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
        public string CameraName { get; set; }
        public string PackageName { get; set; }
        public DateTime NotifyDate { get; set; }
        public bool IsOffWarning { get; set; } //Cờ dành cho package security monitoring khi xác nhận kết thúc vi phạm
        public string EvidenceKey { get; set; }
        public long AreaId { get; set; }
        public string AreaName { get; set; }
        public long SMFenceId { get; set; } //ID hàng rào dành cho package security monitoring
        public string SMFenceName { get; set; } //Tên hàng rào dành cho package security monitoring
        public int TimeDuration { get; set; } // Thời gian cảnh báo dành cho package security monitoring
    }

    public class NotificationSaveModel
    {
        public long CustomerId { get; set; }
        public string CustomerToken { get; set; }
        public long DeviceId { get; set; }
        public long CameraId { get; set; }
        public int PackageNameType { get; set; }
        public long UserViolate { get; set; }
        public long NotifyTime { get; set; }
        public int NotifyType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public int MediaType { get; set; }
        public string BucketMedia { get; set; }
        public string ObjectMedia { get; set; }
        public bool IsOffWarning { get; set; } //Cờ dành cho package security monitoring khi xác nhận kết thúc vi phạm
        public string? EvidenceKey { get; set; }
        public long SMFenceId { get; set; } //ID hàng rào dành cho package security monitoring
        public string SMFenceName { get; set; } //Tên hàng rào dành cho package security monitoring
    }
}
