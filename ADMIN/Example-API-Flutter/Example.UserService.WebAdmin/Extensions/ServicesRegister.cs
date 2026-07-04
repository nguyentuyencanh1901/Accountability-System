using Example.UserService.WebAdmin.Repository;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services;
using Example.UserService.WebAdmin.Services.IServices;

namespace Example.UserService.WebAdmin.Extensions
{
    /// <summary>Đăng ký DI WebAdmin: Repository (gọi API Client) → Service → Controller.</summary>
    public static class ServicesRegister
    {
        public static IServiceCollection AddWebAdminServices(this IServiceCollection services)
        {
            services.AddScoped<IFieldRepository, FieldRepository>();
            services.AddScoped<IExamSetRepository, ExamSetRepository>();
            services.AddScoped<IExamPeriodRepository, ExamPeriodRepository>();
            services.AddScoped<IExamPeriodAssignmentRepository, ExamPeriodAssignmentRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IExamSessionRepository, ExamSessionRepository>();
            services.AddScoped<IExamSessionQuestionRepository, ExamSessionQuestionRepository>();
            services.AddScoped<IExamSessionAnswerRepository, ExamSessionAnswerRepository>();
            services.AddScoped<IExamSessionAnswerOptionRepository, ExamSessionAnswerOptionRepository>();
            services.AddScoped<IAuthenticateRepository, AuthenticateRepository>();

            services.AddScoped<IFieldService, FieldService>();
            services.AddScoped<IExamSetService, ExamSetService>();
            services.AddScoped<IExamPeriodService, ExamPeriodService>();
            services.AddScoped<IExamPeriodAssignmentService, ExamPeriodAssignmentService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IAnswerOptionService, AnswerOptionService>();
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IExamSessionService, ExamSessionService>();
            services.AddScoped<IExamSessionQuestionService, ExamSessionQuestionService>();
            services.AddScoped<IExamSessionAnswerService, ExamSessionAnswerService>();
            services.AddScoped<IExamSessionAnswerOptionService, ExamSessionAnswerOptionService>();
            services.AddScoped<IAuthenticateService, AuthenticateService>();
            services.AddScoped<IHomeService, HomeService>();

            return services;
        }
    }
}
