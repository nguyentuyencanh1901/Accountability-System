using Example.Common.Const;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Example.Common.Utilities.Helper
{
    /// <summary>Tạo key Redis và hỗ trợ header example-refreshcache để bỏ qua cache.</summary>
    public class CacheHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;

        private static readonly Lazy<CacheHelper> lazy = new Lazy<CacheHelper>(() => new CacheHelper());
        public static CacheHelper Instance { get { return lazy.Value; } }

        private CacheHelper()
        {
            _httpContextAccessor = new HttpContextAccessor();
        }

        public bool IsRequestClearCache()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                return false;
            }

            return (context.Request.Headers["example-refreshcache"] + "") == "refreshcache";
        }

        // Hàm tạo key từ danh sách động params
        public string GenerateCacheKey(long customerId, string packageName, params object[] values)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var keyBuilder = new StringBuilder();
            keyBuilder.AppendFormat($"{CacheConst.CachePrefix}:customer_{customerId}:{packageName}:");

            foreach (var value in values)
            {
                // Kiểm tra nếu giá trị là object thì thực hiện Serialize để tạo key
                if (value != null && !IsBasicType(value))
                {
                    string jsonData = JsonSerializer.Serialize(value, JsonSerializerConfigs.GetOption());
                    keyBuilder.AppendFormat($"_{HttpUtility.UrlEncode(jsonData)}");
                }
                else
                {
                    // Nếu là kiểu dữ liệu cơ bản thì chuyển đổi trực tiếp thành string và cộng vào chuỗi key
                    keyBuilder.Append($"_{value?.ToString()}");
                }
            }

            return keyBuilder.ToString();
        }

        public bool IsBasicType(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var type = obj.GetType();
            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.Char:
                case TypeCode.String:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// The ASP.NET Core's RedisCache does not expose the ConnectionMultiplexer required for more advanced Redis scenarios
        /// and it is recommended to have just one ConnectionMultiplexer.
        /// We are left with no option but to use reflection to get a hold of the connection.
        /// </summary>
        public async Task<ConnectionMultiplexer> GetConnectionAsync(RedisCache cache, CancellationToken cancellationToken = default)
        {
            //ensure connection is established
            await ((Task)typeof(RedisCache).InvokeMember("ConnectAsync", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, cache, new object[] { cancellationToken }));

            //get connection multiplexer
            var fi = typeof(RedisCache).GetField("_connection", BindingFlags.Instance | BindingFlags.NonPublic);
            var connection = (ConnectionMultiplexer)fi.GetValue(cache);
            return connection;
        }
    }
}
