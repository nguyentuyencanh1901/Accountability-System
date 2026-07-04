using Example.Common.Utilities;

namespace Example.Common.Const
{
    /// <summary>Cấu hình TTL và prefix key Redis (bật/tắt qua appsettings RedisSettings).</summary>
    public class CacheConst
    {
        public static readonly bool AllowCache = AppSettings.Instance.Get("RedisSettings:AllowCache", false);
        public static readonly int CacheDataExpireOneMinute = 1;
        public static readonly int CacheDataExpireThreeMinute = 3;
        public static readonly int CacheDataExpireFiveMinutes = 5;
        public static readonly int CacheDataExpireTenMinutes = 10;
        public static readonly int CacheDataExpireFifteenMinutes = 15;
        public static readonly int CacheDataExpireShortTime = AppSettings.Instance.Get<int>("RedisSettings:ShortTime", 30);
        public static readonly int CacheDataExpireMediumTime = AppSettings.Instance.Get<int>("RedisSettings:MediumTime", 60);
        public static readonly int CacheDataExpireLongTime = AppSettings.Instance.Get<int>("RedisSettings:LongTime", 90);
        public static readonly int CacheDataExpireOneDay = 1440;
        public static readonly int CacheDataExpireOneWeek = 10080;

        public const string CachePrefix = "software"
            , KeyPrefixSystemConfig = "system-config:"
            , KeyPrefixUserCommon = "user-common:"
            , KeyPrefixUserCenter = "user-center:"
            , KeyPrefixProduct = "product:"
            , KeySystemConfigGetAllByCustomerId = KeyPrefixSystemConfig + "GetAllConfigByCustomerId"
            , KeySystemConfigGetConnectionString = KeyPrefixSystemConfig + "GetConnectionString"
            , KeySystemConfigGetConfigKafka = KeyPrefixSystemConfig + "GetConfigKafka"
            , KeySystemConfigGetConfigSSH = KeyPrefixSystemConfig + "GetConfigSSH"
            , KeySystemConfigGetConfigApiPlatform = KeyPrefixSystemConfig + "GetConfigApiPlatform"
            , KeySystemConfigGetConfigApiCenter = KeyPrefixSystemConfig + "GetConfigApiCenter"
            , KeySystemConfigGetConfigCustomer = KeyPrefixSystemConfig + "GetConfigCustomer"
            , KeySystemConfig_GetAllConfigByType = KeyPrefixSystemConfig + "GetAllConfigByType"
            , KeySystemConfigGetListConfigCustomerOrConfigDefault = KeyPrefixSystemConfig + "GetListConfigCustomerOrConfigDefault"
            , KeySystemConfigGetConfigDomainPortal = KeyPrefixSystemConfig + "GetConfigDomainPortal"
            , KeySystemConfigGetConfigMinIO = KeyPrefixSystemConfig + "GetConfigMinIO"
            , KeyUserCenterGetListSystemAdmin = KeyPrefixUserCenter + "GetListSystemAdmin"
            , KeyUserCenterGetListDeviceByUserId = KeyPrefixUserCenter + "GetListDeviceByUserId"
            , KeyUserCommonGetListAreaByCustomerId = KeyPrefixUserCommon + "GetListAreaByCustomerId"
            , KeyUserCommonGetListDepartmentByCustomerId = KeyPrefixUserCommon + "GetListDepartmentByCustomerId"
            , KeyUserCommonGetListDepartmentByProductId = KeyPrefixUserCommon + "GetListProductId"
            , KeyProductGetListPaging = KeyPrefixProduct + "GetListPaging"
            ;
    }
}
