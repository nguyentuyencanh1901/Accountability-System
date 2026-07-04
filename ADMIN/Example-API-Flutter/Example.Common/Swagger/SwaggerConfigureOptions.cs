using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Example.Common.Swagger
{
    /// <summary>
    /// Configure Swagger
    /// </summary>
    public class SwaggerConfigureOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        /// <summary>
        ///   Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
        /// </summary>
        /// <param name="provider">
        ///   The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.
        /// </param>
        public SwaggerConfigureOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

        /// <inheritdoc/>
        public void Configure(SwaggerGenOptions options)
        {
            // add a swagger document for each discovered API version
            // note: you might choose to skip or document deprecated API versions differently
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            Assembly assem = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            var info = new OpenApiInfo()
            {
                Title = $"{assem.GetName().Name}",
                Version = description.ApiVersion.ToString(),
                Description = "API Docs",
                Contact = new OpenApiContact { Name = "Akacam", Email = "example@fpt.com" },
                License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}
