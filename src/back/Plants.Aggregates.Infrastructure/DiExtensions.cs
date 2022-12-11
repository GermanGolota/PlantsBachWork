using Microsoft.Extensions.DependencyInjection;
using Plants.Aggregates.Infrastructure.Services;
using Plants.Aggregates.Services;

namespace Plants.Aggregates.Infrastructure;

public static class DiExtensions
{
    public static IServiceCollection AddAggregatesInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IEmailer, Emailer>();
        services.AddScoped<IIdentityProvider, IdentityProvider>();
        services.AddScoped<IUserUpdater, UserUpdater>();

        return services;
    }
}
