using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Plants.Presentation.Examples;
using Plants.Shared;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation.Extensions;

public static class DIExtensions
{
    public static IServiceCollection AddPlantsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.BindConfigSections(configuration, typeof(StartupCheckingConfigBinder<>));
        return services;
    }

    private class StartupCheckingConfigBinder<T> : GenericConfigBinder<T> where T : class
    {
        public StartupCheckingConfigBinder(IServiceCollection services, IConfiguration configSection, string optionName)
            : base(services, configSection, optionName)
        {
        }

        public override void AdditionalBinding(OptionsBuilder<T> builder)
        {
            builder.ValidateOnStart();
        }
    }

    public static IServiceCollection AddPlantsSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Plants", Version = "1" });
            c.SwaggerDoc("v2", new OpenApiInfo { Title = "Plants", Version = "2" });
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token **_only_**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };
            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            var requiremenets = new OpenApiSecurityRequirement
            {
                {securityScheme, Array.Empty<string>()}
            };
            c.AddSecurityRequirement(requiremenets);
            c.ExampleFilters();
            c.ResolveConflictingActions(action => action.First());
        });

        services.AddSwaggerExamplesFromAssemblyOf<LoginRequestExample>();
        services.AddApiVersioning(opt =>
        {
            opt.AssumeDefaultVersionWhenUnspecified = true;
            opt.DefaultApiVersion = new(1, 0);
            opt.UseApiBehavior = false;
            opt.ApiVersionReader = new PathApiVersionReader();
        });

        return services;
    }

    private sealed class PathApiVersionReader : IApiVersionReader
    {
        public void AddParameters(IApiVersionParameterDescriptionContext context)
        {
        }

        public string? Read(HttpRequest request)
        {
            var url = new Uri(request.GetDisplayUrl());
            var version = url.Segments.Select(TryGetVersion).Where(x => x.HasValue).Select(x => x.Value).FirstOrDefault();
            return version == default ? null : version.ToString();
        }

        private static int? TryGetVersion(string str)
        {
            int? result;
            if (str.StartsWith('v'))
            {
                var versionStr = str.Substring(1).Replace("/", "");
                if (Int32.TryParse(versionStr, out int parsed))
                {
                    result = parsed;
                }
                else
                {
                    result = null;
                }
            }
            else
            {
                result = null;
            }
            return result;
        }
    }

    public static IApplicationBuilder UsePlantsSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Plants v1");
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "Plants v2");
        });

        return app;
    }

}
