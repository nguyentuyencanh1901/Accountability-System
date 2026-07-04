using Example.Common.Utilities.Mappings;
using Example.Common.Services;
using Example.Common.Services.IServices;
using Example.Common.Grpc.Services;
using Example.UserService.API.Services.IServices;
using Example.UserService.API.Services;
using Example.UserService.API.Repository.IRepository;
using Example.UserService.API.Repository;

namespace Example.UserService.API.Extensions
{
    /// <summary>
    /// Services Register
    /// </summary>
    public static class ServicesRegister
    {
        public static void RegisterCustomServices(this IServiceCollection services)
        {
            services.RegisterMapsterConfiguration();

            services.AddScoped<IKafkaProducerService, KafkaProducerService>();
            services.AddScoped<ITokenService, TokenService>();


            // ==================== SERVICES ====================
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<IAuthenticateService, AuthenticateService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRolePermissionService, RolePermissionService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IAnswerOptionService, AnswerOptionService>();
            services.AddScoped<IFieldService, FieldService>();
            services.AddScoped<IExamSetService, ExamSetService>();
            services.AddScoped<IExamPeriodService, ExamPeriodService>();
            services.AddScoped<IExamPeriodAssignmentService, ExamPeriodAssignmentService>();
            services.AddScoped<IExamSessionService, ExamSessionService>();
            services.AddScoped<IExamSessionQuestionService, ExamSessionQuestionService>();
            services.AddScoped<IExamSessionAnswerService, ExamSessionAnswerService>();
            services.AddScoped<IExamSessionAnswerOptionService, ExamSessionAnswerOptionService>();







            // ==================== REPOSITORIES ====================
            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();

            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
            services.AddScoped<IFieldRepository, FieldRepository>();
            services.AddScoped<IExamSetRepository, ExamSetRepository>();
            services.AddScoped<IExamSetFieldRepository, ExamSetFieldRepository>();
            services.AddScoped<IExamPeriodRepository, ExamPeriodRepository>();
            services.AddScoped<IExamPeriodExamSetRepository, ExamPeriodExamSetRepository>();
            services.AddScoped<IExamPeriodAssignmentRepository, ExamPeriodAssignmentRepository>();
            services.AddScoped<IExamSessionRepository, ExamSessionRepository>();
            services.AddScoped<IExamSessionQuestionRepository, ExamSessionQuestionRepository>();
            services.AddScoped<IExamSessionAnswerRepository, ExamSessionAnswerRepository>();
            services.AddScoped<IExamSessionAnswerOptionRepository, ExamSessionAnswerOptionRepository>();
            // ✅ THÊM CHỖ NÀY
            /* services.AddSingleton<CategoryGrpcClient>();*/

        }
    }
}
