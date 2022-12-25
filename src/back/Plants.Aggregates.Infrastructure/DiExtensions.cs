using Microsoft.Extensions.DependencyInjection;
using Plants.Aggregates.Infrastructure.Helper;
using Plants.Aggregates.Infrastructure.Services;
using Plants.Aggregates.Services;
using Plants.Aggregates.Users;
using Plants.Domain.Infrastructure.Services;

namespace Plants.Aggregates.Infrastructure;

public static class DiExtensions
{
    public static IServiceCollection AddAggregatesInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IMongoClientFactory, MongoClientFactory>();
        services.AddScoped<TempPasswordContext>();
        services.AddScoped<IAuthorizer, Authorizer>();
        services.AddScoped<IIdentityProvider, IdentityProvider>();
        services.AddScoped<IEventStoreClientSettingsFactory, EventStoreClientSettingsFactory>();
        services.AddScoped<EventStoreUserUpdater>();
        services.AddScoped<MongoDbUserUpdater>();
        services.AddScoped<IUserUpdater, UserUpdater>();

        return services;
    }
}
