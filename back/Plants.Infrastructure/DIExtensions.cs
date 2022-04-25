using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Plants.Application.Contracts;
using Plants.Infrastructure.Config;
using Plants.Infrastructure.Services;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Plants.Infrastructure
{
    public static class DIExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<SymmetricEncrypter>();
            services.AddScoped<IJWTokenManager, JWTokenManager>();
            services.BindConfigSection<AuthConfig>(config);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(x =>
           {
               x.RequireHttpsMetadata = false;
               x.SaveToken = true;
               x.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.Get<AuthConfig>().AuthKey)),
                   ValidateIssuer = false,
                   ValidateAudience = false
               };
           });
            services.AddHttpContextAccessor();
            services.AddScoped<PlantsContextFactory>();
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }

        /// <summary>
        /// Would bind a section, that corresponds to linear subdivisioning of 
        /// config into sections using <param name="sectionNames"></param>
        /// If no section names is provided, then an entire config would be used
        /// </summary>
        private static IServiceCollection BindConfigSection<T>(this IServiceCollection services,
          IConfiguration config, params string[] sectionNames) where T : class
        {
            services.Configure<T>(options =>
            {
                var currentConfig = config;
                foreach (var sectionName in sectionNames)
                {
                    currentConfig = currentConfig.GetSection(sectionName);
                }
                currentConfig.Bind(options);
            });
            return services;
        }
    }
}
