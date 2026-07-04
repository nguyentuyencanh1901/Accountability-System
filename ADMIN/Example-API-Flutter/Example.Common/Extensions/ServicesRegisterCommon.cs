using Example.Common.Cache;
using Example.Common.Const;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Example.Common.Extensions
{
    public static class ServicesRegisterCommon
    {
        public static void RegisterCommonServices(this IServiceCollection services)
        {
            RegisterRedisServices(services);
            //SetConfigPostgresSql();
        }

        public static void RegisterRedisServices(this IServiceCollection services)
        {
            var config = new ConfigurationOptions
            {
                //EndPoints = {  },
                User = StaticVariable.RedisAuthName,
                Password = StaticVariable.RedisAuthPassword,
                AbortOnConnectFail = false,
                AllowAdmin = true
            };

            var arrEndpoint = StaticVariable.RedisEndPoints.Split(',');
            foreach (var item in arrEndpoint)
            {
                config.EndPoints.Add(item);
            }

            //Redis Configuration
            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = config;
            });

            services.AddSingleton<IConnectionMultiplexer>(cfg =>
            {
                var connect = ConnectionMultiplexer.Connect(config);
                return connect;
            });

            services.AddScoped<IRedisCache, RedisCache>();
        }

        public static void SetConfigPostgresSql()
        {
            //Fix trying to send a non-UTC DateTime as timestamptz will throw an exception
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        }
    }
}
