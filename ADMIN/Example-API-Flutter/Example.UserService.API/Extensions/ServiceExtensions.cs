using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Example.Common.Const;
using Example.Common.Swagger;
using Example.Common.Utilities;
using System.Text.Json.Serialization;
using System.Text;
using Example.Common.Repository.Interfaces;
using Example.Common.Repository;
using Confluent.Kafka;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Example.Common.Utilities.Helper;
using Microsoft.AspNetCore.SignalR;
using Example.Common.Cache;
using Example.Common.Extensions;
using Example.Common.Entities;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using System.Reflection;
using StackExchange.Redis;
using System.Net;
using Example.UserService.API.DBContexts;

namespace Example.UserService.API.Extensions
{
    /// <summary>Cấu hình hạ tầng API: JWT, Redis, EF, Dapper timezone, CORS.</summary>
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, WebApplicationBuilder builder, ConfigurationManager configuration)
        {
            IWebHostEnvironment env = builder.Environment;

            DapperVietnamTimeConfiguration.Register();

            // IOC for AppSettings
            //AppSettings.Instance.SetConfiguration(configuration);
            AppSettings.Instance.SetConfiguration(builder.Environment.EnvironmentName, configuration);

            services.AddEntityFrameworkIdentityJWT();

            services.AddRateLimiter();

            // truy cập thông tin người dùng thông qua dependency injection
            services.AddHttpContextAccessor();

            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

            //services.AddMassTransit(configuration);

            //var MyAllowedOrigins = "_myAllowedOrigins";
            //services.AddCors(options =>
            //{
            //    options.AddPolicy(MyAllowedOrigins, builder =>
            //    {
            //        builder.WithOrigins(
            //            "http://localhost:4200",
            //            "https://platform.example.io"
            //            ).AllowAnyHeader()
            //            .AllowAnyMethod();
            //    });
            //});

            var MyAllowedOrigins = "_myAllowedOrigins";

            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowedOrigins, builder =>
                {
                    builder.SetIsOriginAllowed(origin =>
                    {
                        // Chấp nhận localhost
                        if (origin == "http://localhost:4200")
                            return true;

                        // Chấp nhận mọi subdomain của abc.com (kể cả abc.com)
                        return origin != null && (origin.EndsWith(".example.io", StringComparison.OrdinalIgnoreCase));
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // Nếu bạn cần gửi cookies hoặc Authorization headers
                });
            });

            services.AddControllers().AddJsonOptions(x =>
            {
                // serialize enums as strings in api responses (e.g. Role)
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                // ignore omitted parameters on models to enable optional params (e.g. User update)
                x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            //services.AddHealthChecks();

            //giới hạn max file upload là 50Mb
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 50 * 1024 * 1024;
            });
            //services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
            //services.AddSingleton<IMessageBusClient, MessageBusClient>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();

            services.AddAndConfigApiVersioning()
                    .AddAndConfigSwagger();

            //services.AddKafka(env);
            services.AddInfrastructureServices();

            // IOC
            services.RegisterCustomServices();
            services.RegisterCommonServices();

            return services;
        }

        public static void AddEntityFrameworkIdentityJWT(this IServiceCollection services)
        {
            // For Entity Framework;
            services.AddDbContext<DataContext>();

            //string validAudience = StaticVariable.JWTConfig.ValidAudience;
            //string validIssuer = StaticVariable.JWTConfig.ValidIssuer;
            //// Adding Authentication
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            //// Adding Jwt Bearer
            //.AddJwtBearer(options =>
            //{
            //    options.SaveToken = true;
            //    options.RequireHttpsMetadata = false;
            //    options.TokenValidationParameters = new TokenValidationParameters()
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidateLifetime = true,
            //        ValidateIssuerSigningKey = true,
            //        ClockSkew = TimeSpan.Zero,

            //        ValidAudience = validAudience,
            //        ValidIssuer = validIssuer,
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(StaticVariable.JWTConfig.Secret))
            //    };
            //    options.Events = new JwtBearerEvents
            //    {
            //        OnMessageReceived = context =>
            //        {
            //            var accessToken = context.Request.Query["access_token"];

            //            // If the request is for our hub...
            //            var path = context.HttpContext.Request.Path;
            //            if (!string.IsNullOrEmpty(accessToken) &&
            //                (path.StartsWithSegments("/notificationHub")))
            //            {
            //                // Read the token out of the query string
            //                context.Token = accessToken;
            //            }
            //            return Task.CompletedTask;
            //        }
            //    };
            //});
        }

        //private static void AddMassTransit(this IServiceCollection services, ConfigurationManager configuration)
        //{
        //    var rabbitMqSettings = configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();
        //    services.AddMassTransit(mt =>
        //        mt.UsingRabbitMq((cntxt, cfg) =>
        //        {
        //            cfg.Host(rabbitMqSettings.Uri, "/", c =>
        //            {
        //                c.Username(rabbitMqSettings.UserName);
        //                c.Password(rabbitMqSettings.Password);
        //            });

        //            cfg.ReceiveEndpoint("queue-order", (c) =>
        //            {
        //                c.Consumer<CommandMessageConsumer>();
        //            });
        //        }));
        //}

        public static void AddKafka(this IServiceCollection services, IWebHostEnvironment env = null)
        {
            SecurityProtocol securityProtocol = SecurityProtocol.SaslPlaintext;
            if (env != null)
            {
                securityProtocol = env.EnvironmentName.Equals("Development") ? SecurityProtocol.SaslPlaintext : SecurityProtocol.SaslSsl;
            }
            var producerConfiguration = new ProducerConfig()
            {
                BootstrapServers = StaticVariable.KafkaSettingsModel.Bootstrapservers,
                //SecurityProtocol = SecurityProtocol.SaslPlaintext,
                //SecurityProtocol = SecurityProtocol.Plaintext,
                SecurityProtocol = securityProtocol,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = StaticVariable.KafkaSettingsModel.SaslUsername,
                SaslPassword = StaticVariable.KafkaSettingsModel.SaslPassword,
                SslCaLocation = FileHelper.GetFileSSL(), // Đường dẫn tới tệp chứng chỉ CA (nếu sử dụng SSL)
            };
            services.AddSingleton<ProducerConfig>(producerConfiguration);
        }

        //private static IServiceCollection ConfigureUserDbContext(this IServiceCollection services, IConfiguration configuration)
        //{
        //    var connectionString = configuration.GetConnectionString("DefaultConnectionString");
        //    var builder = new MySqlConnectionStringBuilder(connectionString);
        //    services.AddDbContext<DataContext>( m => m.UseMySql(builder.ConnectionString,
        //    ServerVersion.AutoDetect(builder.ConnectionString), mySqlOptionsAction: e =>
        //    {
        //        e.MigrationsAssembly("User.API");
        //        e.SchemaBehavior(MySqlSchemaBehavior.Ignore);
        //    }));
        //    return services;
        //}

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            return services.AddScoped(typeof(IRepositoryBaseAsync<,,>), typeof(RepositoryBaseAsync<,,>))
            .AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

            //return services.AddScoped(typeof(IRepositoryBaseAsync<,,>), typeof(RepositoryBaseAsyncAsync<,,>))
            //.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>)) // IServiceCollection
            //.AddScoped<ICustomerRepository, CustomerRepository>();
        }
        public static void AddRateLimiter(this IServiceCollection services)
        {
            if (StaticVariable.RateLimitingModel != null && StaticVariable.RateLimitingModel.EnableRateLimiting)
            {
                // Configure Rate Limiting
                services.AddRateLimiter(options =>
                {
                    //// Tạo Attribute để áp dụng cho từng API (Đặt [EnableRateLimiting("fixed")] vào API trong Controller)
                    //options.AddFixedWindowLimiter("fixed", opt =>
                    //{
                    //    opt.Window = TimeSpan.FromMinutes(1); // Khoảng thời gian giới hạn
                    //    opt.PermitLimit = 5; // Số lượng request tối đa trong khoảng thời gian
                    //    opt.QueueLimit = 2; // Số lượng request có thể được xếp hàng chờ khi đạt giới hạn
                    //    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // Thứ tự xử lý các request trong hàng đợi
                    //});

                    var partitionKey = string.Empty;
                    // Áp dụng chính sách Rate Limiting cho tất cả các API (VD: 1 phút mỗi API được reqest 20 lần)
                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    {
                        if (!(httpContext?.GetEndpoint()?.DisplayName ?? "").StartsWith("gRPC"))
                        {
                            var endpoint = httpContext?.Request.Path.ToString().ToLower();
                            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                            partitionKey = $"{ipAddress}-{endpoint}";

                            return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                            {
                                Window = TimeSpan.FromSeconds(StaticVariable.RateLimitingModel.TimeSeconds), // Khoảng thời gian giới hạn
                                PermitLimit = StaticVariable.RateLimitingModel.PermitLimit, // Số lượng request tối đa trong khoảng thời gian
                                                                                            //QueueLimit = 2, // Số lượng request có thể được xếp hàng chờ khi đạt giới hạn
                                                                                            //QueueProcessingOrder = QueueProcessingOrder.OldestFirst // Thứ tự xử lý các request trong hàng đợi
                            });
                        }
                        else
                        {
                            // Nếu yêu cầu từ gRPC, không áp dụng giới hạn
                            partitionKey = "gRPC";
                            return RateLimitPartition.GetNoLimiter(partitionKey);
                        }
                    });

                    // Cấu hình mã trạng thái từ chối và xử lý sự kiện từ chối
                    options.RejectionStatusCode = 429;
                    options.OnRejected = async (context, token) =>
                    {
                        var message = BuildRateLimitResponseMessage(context);
                        context.HttpContext.Response.StatusCode = 429;
                        await context.HttpContext.Response.WriteAsync(message);
                    };
                });

                string BuildRateLimitResponseMessage(OnRejectedContext onRejectedContext)
                {
                    var hostName = onRejectedContext.HttpContext.Request.Headers.Host.ToString();
                    return $"You have reached the maximum number of requests allowed for the IP address ({hostName}).";
                }
            }
        }
    }

    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.Identity?.Name;
        }
    }
}
