using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;

namespace Example.Common.Extensions
{
    public static class PrometheusHealthCheckResponseWriter
    {
        public const string ContentType = "text/plain";
        public static Task WritePrometheusResultText(HttpContext context, HealthReport report, IWebHostEnvironment env)
        {
            var output = new StringBuilder();

            string status = report.Status switch
            {
                HealthStatus.Healthy => "0",
                HealthStatus.Degraded => "1",
                HealthStatus.Unhealthy => "2",
                _ => "2"
            };

            output.AppendLine($"# HELP health_status Health status of the API (0 = Healthy, 1 = Degraded, 2 = Unhealthy)");
            output.AppendLine($"# TYPE health_status gauge");
            output.AppendLine($"health_status{{status=\"{report.Status}\", app=\"{env.ApplicationName}\"}} {status}");

            return context.Response.WriteAsync(output.ToString());
        }
    }

}
