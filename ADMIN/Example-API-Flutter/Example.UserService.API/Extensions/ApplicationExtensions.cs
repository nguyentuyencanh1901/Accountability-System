using Example.Common.Const;
using Example.Common.Extensions;
using Example.Common.Utilities.ErroHandler;
using Example.Common.Utilities.Helper;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;

namespace Example.UserService.API.Extensions
{
    public static class ApplicationExtensions
    {
        public static void UseInfrastructure(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //else
            //{
            //    app.UseHsts();
            //}

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            //app.UseHttpsRedirection();

            var uploadRoot = EntityImageHelper.GetUploadRoot();
            if (!Directory.Exists(uploadRoot))
            {
                Directory.CreateDirectory(uploadRoot);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadRoot),
                RequestPath = EntityImageHelper.UploadRequestPath
            });

            //app.UseMiddleware<DomainHandlerMiddleware>();

            app.UseRouting();

            // Authentication & Authorization
            //app.UseAuthentication();
            //app.UseAuthorization();

            // global error handler
            app.UseMiddleware<ErrorHandlerMiddleware>();

            if (StaticVariable.RateLimitingModel != null && StaticVariable.RateLimitingModel.EnableRateLimiting)
            {
                app.UseRateLimiter();
            }

            app.UseEndpoints(endpoints =>
            {
                // Configure the HTTP request pipeline.
                endpoints.MapControllers();
            });
        }
    }
}
