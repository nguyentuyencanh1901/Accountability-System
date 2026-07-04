using System.ComponentModel;

namespace Example.Common.Enums
{
    public enum NotifyType
    {
        [Description("System")]
        System = 1,
        [Description("Package")]
        Package = 2
    }

    public enum NotifyMediaType
    {
        [Description("Image")]
        Image = 1,
        [Description("Video")]
        Video = 2,
        [Description("Audio")]
        Audio = 3
    }

    public enum NotificationB2CTypeEnum
    {
        [Description("Create Camera")]
        CreateManual = 1,
        [Description("Update Camera")]
        UpdateManual = 2,
        [Description("Change Status Camera")]
        ChangeStatus = 3,
        [Description("Scan Camera")]
        ScanCamera = 4,
        [Description("Verify Camera")]
        VerifyCamera = 5,
        [Description("Refresh Image")]
        RefreshImage = 6,
        [Description("Create Zone")]
        CreateZone = 7,
        [Description("Update Zone")]
        UpdateZone = 8,
        [Description("Delete Zone")]
        DeleteZone = 9,
        [Description("Create Config")]
        CreateConfig = 10,
        [Description("Update Config")]
        UpdateConfig = 11,
        [Description("Delete Config")]
        DeleteConfig = 12,
        [Description("Health Check Camera")]
        HealthCheckCamera = 13
    }
}
