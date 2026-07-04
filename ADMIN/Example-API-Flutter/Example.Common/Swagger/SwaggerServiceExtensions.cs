using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Example.Common.Swagger
{
    public static class SwaggerServiceExtensions
    {
        public static IServiceCollection AddAndConfigApiVersioning(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });

            services.AddApiVersioning(
                    options =>
                    {
                        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                        options.AssumeDefaultVersionWhenUnspecified = true;
                        // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                        options.ReportApiVersions = true;
                    })
                .AddApiExplorer(
                    options =>
                    {
                        // add the versioned api explorer, which also adds
                        // IApiVersionDescriptionProvider service
                        // note: the specified format code will format the version as "'v'major[.minor][-status]"
                        options.GroupNameFormat = "'v'VVV";

                        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                        // can also be used to control the format of the API version in route templates
                        options.SubstituteApiVersionInUrl = true;
                    })
                ;
            // this enables binding ApiVersion as a endpoint callback parameter. if you don't use
            // it, then you should remove this configuration.
            //.EnableApiVersionBinding()

            return services;
        }

        public static IServiceCollection AddAndConfigSwagger(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigureOptions>();
            services.AddSwaggerGen(options =>
            {
                // add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();

                // JWT Bearer Authorization
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
                });

                //options.AddSecurityDefinition("CustomerId", new OpenApiSecurityScheme
                //{
                //    In = ParameterLocation.Header,
                //    Description = "Please enter CustomerId",
                //    Name = "CustomerId",
                //    Type = SecuritySchemeType.ApiKey
                //});

                //options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                //{
                //    new OpenApiSecurityScheme
                //    {
                //        Reference = new OpenApiReference
                //        {
                //            Type = ReferenceType.SecurityScheme,
                //            Id = "CustomerId"
                //        }
                //    },
                //    new List<string>()
                //}
                //});

                // add header parameter 
                //options.OperationFilter<AddRequiredHeaderParameter>();

                //options.OperationFilter<RemoveHeaderParamsOperationFilter>(); // Sử dụng bộ lọc để loại bỏ các tham số trên Header

                Assembly assem = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

                var xmlFile = $"{assem.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            return services;
        }

        public static void UseConfigSwagger(this WebApplication app)
        {
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                foreach (var description in app.DescribeApiVersions())
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
        }
    }
}
