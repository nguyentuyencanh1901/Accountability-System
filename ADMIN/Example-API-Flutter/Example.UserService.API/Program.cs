
using Serilog;
using Example.Common.Logging;
using Example.Common.Swagger;
using Example.UserService.API.Extensions;
using Example.UserService.API.DBContexts;
using Example.UserService.API.Hubs;
using Example.Common.Const;
using Elastic.Apm.NetCoreAll;
using OfficeOpenXml;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;


// ✅ SET LICENSE CHO EPPLUS 8+
ExcelPackage.License.SetNonCommercialPersonal("Canh");

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information($"Start {builder.Environment.ApplicationName} - {builder.Environment.EnvironmentName} up");

try
{
    builder.Host.UseSerilog(Serilogger.Configure);
    // Add services to the container.
    builder.Services.AddInfrastructure(builder, configuration);
    builder.Services.AddGrpc();

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Listen(IPAddress.Any, 5000, o =>
        {
            o.Protocols = HttpProtocols.Http1; // REST
        });

        options.Listen(IPAddress.Any, 5001, o =>
        {
            o.Protocols = HttpProtocols.Http2; // gRPC ✅
        });
    });

    //JWT
    var jwtSection = builder.Configuration.GetSection("JWT");

    var issuer = jwtSection["ValidIssuer"];
    var audience = jwtSection["ValidAudience"];
    var secret = jwtSection["Secret"];

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection("JWT");
        var secret = jwtSection["Secret"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret!)
            )
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {        
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("❌ JWT ERROR: " + context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    WebApplication app = builder.Build();

    // Cấu hình Elastic APM
    if (StaticVariable.EnabledAMP)
    {
        app.UseAllElasticApm(configuration);
    }

    app.MapHub<NotificationHub>("/notificationHub");

    if (app.Environment.IsDevelopment())
    {
        // Add default token for environment DEV
        app.Use(async (context, next) =>
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (authHeader == null)
            {
                //context.Request.Headers.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJmOTRmMzE0Ny0zNWU1LTQ0YzYtYWRmMy04MmQ4Y2E0NGM3NTgiLCJodHRwczovL2ZwdC5jb20vaWRlbnRpdHkvY2xhaW1zL3VzZXJpZCI6IjIiLCJodHRwczovL2ZwdC5jb20vaWRlbnRpdHkvY2xhaW1zL2N1c3RvbWVyaWQiOiIwIiwiaHR0cHM6Ly9mcHQuY29tL2lkZW50aXR5L2NsYWltcy91c2VydHlwZSI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWthY2FtIiwiaHR0cHM6Ly9mcHQuY29tL2lkZW50aXR5L2NsYWltcy9mdWxsbmFtZSI6ImFrYUNhbSIsImh0dHBzOi8vZnB0LmNvbS9pZGVudGl0eS9jbGFpbXMvZW1haWwiOiJha2FjYW1AZnB0LmNvbSIsImh0dHBzOi8vZnB0LmNvbS9pZGVudGl0eS9jbGFpbXMvYXJlYXMiOiJbXSIsImh0dHBzOi8vZnB0LmNvbS9pZGVudGl0eS9jbGFpbXMvZGVwYXJ0bWVudHMiOiJbXSIsImF1ZCI6WyJhcGkuZ2F0ZXdheS5jb20iLCJhcGkucHJvZHVjdC5jb20iLCJhcGkuZ2F0ZXdheS5jb20iXSwiZXhwIjoxNzUzMjU3NjA5LCJpc3MiOiJhcGkudXNlci5jb20ifQ.EU1ztR0X9Ljmri17J8T1RowjWoCjin9_s4e2m9yZ5ig");
            }
            await next();
        });
    }

    app.UseConfigSwagger();

    app.UseRouting(); // ✅ thêm dòng này

    app.UseAuthentication();
    app.UseAuthorization();

    // nếu có custom middleware thì để sau
    app.UseInfrastructure(builder.Environment);

    // map controller (nếu chưa có)
    app.MapControllers();

    try
    {
        await app.SeedPermissionsAsync();
        await app.SeedSuperAdminAsync();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Không thể đồng bộ quyền hạn / tài khoản SuperAdmin — kiểm tra kết nối database.");
    }

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Program Exception: {ex.Message}");
    string type = ex.GetType().Name;
    if (type.Equals(value: "Stop The HostException", StringComparison.Ordinal))
    {
        // Migrate db
        //throw;
    }

    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information($"Shut down {builder.Environment.ApplicationName} complete");
    Log.CloseAndFlush();
}
