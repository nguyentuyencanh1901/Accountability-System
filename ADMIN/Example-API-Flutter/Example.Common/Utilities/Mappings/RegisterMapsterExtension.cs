using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Example.Common.Utilities.Mappings
{
    /// <summary>
    /// Register Mapster Extension
    /// </summary>
    public static class RegisterMapsterExtension
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            Assembly assem = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            // scans the assembly and gets the IRegister, adding the registration to the TypeAdapterConfig
            typeAdapterConfig.Scan(assem);
            // register the mapper as Singleton service for my application
            var mapperConfig = new Mapper(typeAdapterConfig);
            services.AddSingleton<IMapper>(mapperConfig);
        }
    }
}