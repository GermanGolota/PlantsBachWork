using Microsoft.Extensions.DependencyInjection;

namespace Plants.Services.Infrastructure;

public static class DiExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddScoped<SymmetricEncrypter>();
        services.AddScoped<IJWTokenManager, JWTokenManager>();
        services.AddScoped<IEmailer, MockEmailer>();
        return services;
    }
}