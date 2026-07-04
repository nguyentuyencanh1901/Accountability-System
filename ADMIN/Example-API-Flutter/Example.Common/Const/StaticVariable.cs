using Example.Common.Models.AppSetting;
using Example.Common.Utilities;

namespace Example.Common.Const
{
    public static class StaticVariable
    {
        //public static string StaticVersion { get; set; }
        //public static string WebRootPath;
        public static readonly DatabaseModel Databases = AppSettings.Instance.Get<DatabaseModel>("Databases");
        //public static readonly AppConfigModel AppConfig = AppSettings.Instance.Get<AppConfigModel>("AppConfig");
        public static readonly JWTModel JWTConfig = AppSettings.Instance.Get<JWTModel>("JWT");
        public static readonly KafkaSettingsModel KafkaSettingsModel = AppSettings.Instance.Get<KafkaSettingsModel>("KafkaSettings");
        public static readonly bool AllowConsumerLogMessage = AppSettings.Instance.Get<bool>("AllowConsumerLogMessage");
        public static readonly ElasticSearchModel ElasticSearchModel = AppSettings.Instance.Get<ElasticSearchModel>("ElasticConfiguration");
        public static readonly bool EnabledAMP = AppSettings.Instance.Get<bool>("ElasticApm:EnabledAMP");
        public static readonly RateLimitingModel RateLimitingModel = AppSettings.Instance.Get<RateLimitingModel>("RateLimiting");

        #region Caching

        public static readonly bool AllowRedisCache = AppSettings.Instance.Get<bool>("RedisSettings:AllowCache");
        public static readonly string RedisEndPoints = AppSettings.Instance.Get<string>("RedisSettings:EndPoints", "redis_example:6379");
        public static readonly string RedisAuthName = AppSettings.Instance.Get<string>("RedisSettings:AuthName", "");
        public static readonly string RedisAuthPassword = AppSettings.Instance.Get<string>("RedisSettings:AuthPassword", "");
        //public static readonly Dictionary<string, int> UrlExpireTimesDic = AppSettings.GetObject<Dictionary<string, int>>("Cache:Redis:Page:UrlExpireTimes");
        //public static readonly int DefaultExpireTime = AppSettings.Instance.Get<int>("Cache:Redis:Page:DefaultExpireTime", 36000);

        public static readonly int CacheDataExpireShortTime = AppSettings.Instance.Get<int>("RedisSettings:ShortTime", 30);
        public static readonly int CacheDataExpireMediumTime = AppSettings.Instance.Get<int>("RedisSettings:MediumTime", 60);
        public static readonly int CacheDataExpireLongTime = AppSettings.Instance.Get<int>("RedisSettings:LongTime", 90);
        public const int CacheDataExpireTimeDefault = 600;

        #endregion Caching

        #region
        public static readonly string apiDownloadImage = AppSettings.Instance.Get<string>("apiConnect:apiDownloadImage");
        public static readonly string apiConvertImageToVector = AppSettings.Instance.Get<string>("apiConnect:apiConvertImageToVector");
        public static readonly string apiConvertImageToAI = AppSettings.Instance.Get<string>("apiConnect:apiConvertImageToAI");
        #endregion

        #region MinIO
        public static readonly UploadFileModel minIOConfig = AppSettings.Instance.Get<UploadFileModel>("MinIOSettings");
        public static readonly string DomainMinioProxy = AppSettings.Instance.Get<string>("MinioProxy", "").TrimEnd('/');
        #endregion

        #region Common Settings
        public static readonly CommonSettingModel CommonSetting = AppSettings.Instance.Get<CommonSettingModel>("CommonSettings");
        #endregion
        // Format Date
        //public static string FormatDate = AppSettings.Instance.GetString("FormatDate", "");
        //public const string FormatDateSitemap = "yyyy-MM-ddTHH\\:mm\\:sszzz";

        public const int UserRoleDataAllowAll = -1;
        public const string Admin = "example";
        public const string RoleCommonName = "Customer Common";
        public const string DefaultPassword = "2ae512d5-B52a-40de-ae86-08e80ce74ce7";

        public const string DateTimeFormatSQL = "yyyy-MM-dd HH:mm:ss.ffffff";
        public const string FormatDateVn = "dd-MM-yyyy";

        public const string HeaderConnectString = "HeaderConnectString";
        public const string DataTypeJson = "json";
        public const string DataTypeString = "string";

        public const string ApiVersionV1 = "1";
        public const string ApiVersionV100 = "100";
        public const string SecurityChain = "******";
        public static readonly string PrefixPasswordUserDefault = AppSettings.Instance.Get<string>("PrefixPasswordUserDefault");

        public const string CustomerIdKey = "customerId";

        public static readonly AuthSettingModel AuthSetting = AppSettings.Instance.Get<AuthSettingModel>("AuthSetting");
        public static readonly SendEmailSettingModel SendEmailSetting = AppSettings.Instance.Get<SendEmailSettingModel>("SendEmailSetting");

        public static readonly string CustomerApi = AppSettings.Instance.Get<string>("apiConnect:CustomerApi");
        public static readonly string CustomerApiPath = AppSettings.Instance.Get<string>("apiConnect:CustomerApiPath", "/api/v1");

        public static readonly string GrpcUserAddress = AppSettings.Instance.Get<string>("Grpc:User:Address", "http://platform-user-api");
        public static readonly int GrpcUserPort = AppSettings.Instance.Get<int>("Grpc:User:Port", 6102);
        public static readonly string GrpcUser = $"{GrpcUserAddress}:{GrpcUserPort}";

        public static readonly string GrpcCustomerDectectionAddress = AppSettings.Instance.Get<string>("Grpc:CustomerDectection:Address", "http://platform-customer-dectection-api");
        public static readonly int GrpcCustomerDectectionPort = AppSettings.Instance.Get<int>("Grpc:CustomerDectection:Port", 6103);
        public static readonly string GrpcCustomerDectection = $"{GrpcCustomerDectectionAddress}:{GrpcCustomerDectectionPort}";

        public static readonly string GrpcCenterAddress = AppSettings.Instance.Get<string>("Grpc:Center:Address", "http://platform-center-api");
        public static readonly int GrpcCenterPort = AppSettings.Instance.Get<int>("Grpc:Center:Port", 2070);
        public static readonly string GrpcCenter = $"{GrpcCenterAddress}:{GrpcCenterPort}";
    }
}
