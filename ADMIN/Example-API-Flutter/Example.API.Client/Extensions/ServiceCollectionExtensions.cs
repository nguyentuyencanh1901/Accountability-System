using Example.API.Client.Clients.AppUser;
using Example.API.Client.Clients.AnswerOption;
using Example.API.Client.Clients.Authenticate;
using Example.API.Client.Clients.ExamPeriod;
using Example.API.Client.Clients.ExamPeriodAssignment;
using Example.API.Client.Clients.ExamSession;
using Example.API.Client.Clients.ExamSessionAnswer;
using Example.API.Client.Clients.ExamSessionAnswerOption;
using Example.API.Client.Clients.ExamSessionQuestion;
using Example.API.Client.Clients.ExamSet;
using Example.API.Client.Clients.Field;
using Example.API.Client.Clients.Permission;
using Example.API.Client.Clients.Question;
using Example.API.Client.Clients.Role;
using Example.API.Client.Configuration;
using Example.API.Client.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Example.API.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>Đăng ký tất cả API Client vào DI container</summary>
        public static IServiceCollection AddExampleApiClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiClientOptions>(configuration.GetSection(ApiClientOptions.SectionName));
            services.AddScoped<IApiTokenProvider, ApiTokenProvider>();

            RegisterHttpClient<IAuthenticateClient, AuthenticateClient>(services, configuration);
            RegisterHttpClient<IAppUserClient, AppUserClient>(services, configuration);
            RegisterHttpClient<IRoleClient, RoleClient>(services, configuration);
            RegisterHttpClient<IPermissionClient, PermissionClient>(services, configuration);
            RegisterHttpClient<IQuestionClient, QuestionClient>(services, configuration);
            RegisterHttpClient<IAnswerOptionClient, AnswerOptionClient>(services, configuration);
            RegisterHttpClient<IExamSetClient, ExamSetClient>(services, configuration);
            RegisterHttpClient<IFieldClient, FieldClient>(services, configuration);
            RegisterHttpClient<IExamPeriodClient, ExamPeriodClient>(services, configuration);
            RegisterHttpClient<IExamPeriodAssignmentClient, ExamPeriodAssignmentClient>(services, configuration);
            RegisterHttpClient<IExamSessionClient, ExamSessionClient>(services, configuration);
            RegisterHttpClient<IExamSessionQuestionClient, ExamSessionQuestionClient>(services, configuration);
            RegisterHttpClient<IExamSessionAnswerClient, ExamSessionAnswerClient>(services, configuration);
            RegisterHttpClient<IExamSessionAnswerOptionClient, ExamSessionAnswerOptionClient>(services, configuration);

            return services;
        }

        private static void RegisterHttpClient<TClient, TImplementation>(
            IServiceCollection services,
            IConfiguration configuration)
            where TClient : class
            where TImplementation : class, TClient
        {
            var baseUrl = configuration.GetSection(ApiClientOptions.SectionName)["BaseUrl"]
                ?? "http://localhost:5000";

            services.AddHttpClient<TClient, TImplementation>(client =>
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
        }
    }
}
