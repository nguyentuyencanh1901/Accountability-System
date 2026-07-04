using System.ComponentModel;

namespace Example.Common.Enums
{
    public enum KafkaActionEnum
    {
        [Description("Create")]
        Create = 1,
        [Description("Update")]
        Update = 2,
        [Description("Delete")]
        Delete = 3,
        [Description("SyncDataAI")]
        SyncDataAI = 4, // Consumer trên Box sẽ call API AI để xóa cache
        [Description("VerifyCamera")]
        VerifyCamera = 5,
        [Description("RefreshImage")]
        RefreshImage = 6,
        [Description("HealthCheckCamera")] // Kiểm tra URL camera còn hoạt động không
        HealthCheckCamera = 7
    }
}
