using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Plants.Application.Contracts;
using Plants.Infrastructure.Config;
using Plants.Infrastructure.Helpers;
using Plants.Infrastructure.Services;
using System.Linq;
using System.Text;

namespace Plants.Infrastructure
{
    public static class DIExtensions
    {
        const string AuthSectionName = "Auth";
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<PlantsContextFactory>();
            services.AddAuth(config)
                .AddServices();
            return services;
        }

        private static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
        {
            string key = GetAuthKey(config);
            services.AddScoped<SymmetricEncrypter>();
            services.AddScoped<IJWTokenManager, JWTokenManager>();
            services.BindConfigSection<AuthConfig>(config, AuthSectionName);
            services.BindConfigSection<ConnectionConfig>(config);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(x =>
           {
               x.RequireHttpsMetadata = false;
               x.SaveToken = true;
               x.TokenValidationParameters = GetValidationParams(key);
           });
            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IStatsService, StatsService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IInfoService, InfoService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IPlantsService, PlantsService>();
            services.AddScoped<IOrdersService, OrdersService>();
            return services;
        }

        public static TokenValidationParameters GetValidationParams(string key)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        }

        public static string GetAuthKey(IConfiguration config)
        {
            return config
                .GetSection(AuthSectionName)
                .Get<AuthConfig>()
                .AuthKey;
        }

        /// <summary>
        /// Would bind a section, that corresponds to linear subdivisioning of 
        /// config into sections using <param name="sectionNames"></param>
        /// If no section names is provided, then an entire config would be used
        /// </summary>
        public static IServiceCollection BindConfigSection<T>(this IServiceCollection services,
          IConfiguration config, params string[] sectionNames) where T : class
        {
            services.Configure<T>(options =>
            {
                sectionNames
                    .Aggregate(config, (config, sectionName) => config.GetSection(sectionName))
                    .Bind(options);
            });
            return services;
        }
    }
}
