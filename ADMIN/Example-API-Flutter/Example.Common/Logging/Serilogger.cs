using Example.Common.Const;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Example.Common.Logging
{
    public static class Serilogger
    {
        public static Action<HostBuilderContext, LoggerConfiguration> Configure => (context, configuration) =>
        {
            var applicationName = context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-");
            var environmentName = context.HostingEnvironment.EnvironmentName ?? "Development";

            // Create a service provider to resolve IHttpContextAccessor
            var services = new ServiceCollection();
            services.AddHttpContextAccessor();
            var serviceProvider = services.BuildServiceProvider();
            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

            string outputTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}";

            // Cấu hình Serilog
            configuration.WriteTo.Debug()
            .WriteTo.Console(outputTemplate: outputTemplate)
            .Enrich.FromLogContext()
            .Enrich.With(new CustomEnricher(httpContextAccessor))
            .Enrich.WithMachineName()
            .Enrich.WithProperty("CustomerName", "Example")
            .Enrich.WithProperty("DeviceName", "Platform")
            .Enrich.WithProperty("Environment", environmentName)
            .Enrich.WithProperty("Application", applicationName)
            .ReadFrom.Configuration(context.Configuration);

            if (StaticVariable.ElasticSearchModel != null && StaticVariable.ElasticSearchModel.EnableLogElasticsearch)
            {
                // Đường dẫn đến chứng chỉ của bạn
                string appDataPath = Path.Combine(AppContext.BaseDirectory, "SSL");
                string certificatePath = Path.Combine(appDataPath, StaticVariable.ElasticSearchModel.CertificateFile);
                string certificatePassword = StaticVariable.ElasticSearchModel.CertificatePassword;

                string indexFormat = $"{Assembly.GetExecutingAssembly()?.GetName()?.Name?.ToLower().Replace(".", "-")}-{environmentName.ToLower()}-{DateTime.UtcNow:yyyy-MM}";

                // Cấu hình ElasticsearchSinkOptions với chứng chỉ SSL
                ElasticsearchSinkOptions ConfigureElasticSink()
                {
                    return new ElasticsearchSinkOptions(new Uri(StaticVariable.ElasticSearchModel.Uri))
                    {
                        ModifyConnectionSettings = x => x.BasicAuthentication(StaticVariable.ElasticSearchModel.Username, StaticVariable.ElasticSearchModel.Password)
                                                         .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
                                                         .ClientCertificate(new X509Certificate2(certificatePath, certificatePassword)),
                        AutoRegisterTemplate = true,
                        IndexFormat = indexFormat,
                        NumberOfReplicas = 1,
                        NumberOfShards = 2
                    };
                }

                configuration.WriteTo.Elasticsearch(ConfigureElasticSink());
            }

            //bool enableFileLogging = true;
            //if (enableFileLogging)
            //{
            //    configuration.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate, rollOnFileSizeLimit: true);
            //}

            //// In ra bất kỳ lỗi nào xảy ra khi ghi nhật ký vào Elasticsearch.
            //SelfLog.Enable(Console.Error);
        };

    }

    public class CustomEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomEnricher(IHttpContextAccessor? httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Exception != null)
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var request = _httpContextAccessor.HttpContext.Request;
                    //logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("URL", request.Path.ToString()));
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("URLReferrer", request.Headers["Referer"].ToString()));
                }

                // Phân tích thông tin Exception
                var exception = logEvent.Exception;

                // ExceptionType
                var exceptionType = propertyFactory.CreateProperty("ExceptionType", exception.GetType().FullName);
                logEvent.AddPropertyIfAbsent(exceptionType);

                // Message
                var message = propertyFactory.CreateProperty("Message", exception.Message);
                logEvent.AddPropertyIfAbsent(message);

                // Source
                var source = propertyFactory.CreateProperty("Source", exception.Source);
                logEvent.AddPropertyIfAbsent(source);

                // StackTrace
                var stackTrace = propertyFactory.CreateProperty("StackTrace", exception.StackTrace);
                logEvent.AddPropertyIfAbsent(stackTrace);
            }
        }
    }
}
