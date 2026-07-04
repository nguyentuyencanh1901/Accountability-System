using Example.UserService.WebApp.Repository;
using Example.UserService.WebApp.Repository.IRepository;
using Example.UserService.WebApp.Services;
using Example.UserService.WebApp.Services.IServices;

namespace Example.UserService.WebApp.Extensions
{
    /// <summary>Đăng ký DI WebApp: Repository (gọi API Client) → Service → Controller.</summary>
    public static class ServicesRegister
    {
        public static IServiceCollection AddWebAppServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticateRepository, AuthenticateRepository>();
            services.AddScoped<IExamSessionRepository, ExamSessionRepository>();
            services.AddScoped<IExamSetRepository, ExamSetRepository>();
            services.AddScoped<IExamPeriodAssignmentRepository, ExamPeriodAssignmentRepository>();
            services.AddScoped<IAppUserRepository, AppUserRepository>();

            services.AddScoped<IAuthenticateService, AuthenticateService>();
            services.AddScoped<IHomeService, HomeService>();
            services.AddScoped<IExamSetService, ExamSetService>();
            services.AddScoped<IExamSessionService, ExamSessionService>();
            services.AddScoped<IProfileService, ProfileService>();

            return services;
        }
    }
}
