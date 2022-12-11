using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Plants.Application.Contracts;
using Plants.Infrastructure.Config;
using Plants.Infrastructure.Helpers;
using Plants.Infrastructure.Services;
using System.Text;

namespace Plants.Infrastructure;

public static class DIExtensions
{
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
        services.AddScoped<IEmailer, Emailer>();
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
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IInstructionsService, InstructionsService>();
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
            .GetSection(AuthConfig.Section)
            .Get<AuthConfig>()
            .AuthKey;
    }
}
