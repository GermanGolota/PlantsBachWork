using Microsoft.Extensions.DependencyInjection;
using Plants.Aggregates.Infrastructure.Domain;
using Plants.Aggregates.Infrastructure.Helper;
using Plants.Aggregates.Infrastructure.Helper.ElasticSearch;
using Plants.Aggregates.Infrastructure.Services;
using Plants.Aggregates.Services;
using Plants.Aggregates.Users;
using Plants.Domain.Infrastructure.Services;

namespace Plants.Aggregates.Infrastructure;

public static class DiExtensions
{
    public static IServiceCollection AddAggregatesInfrastructure(this IServiceCollection services)
    {
        services.AddDomainDependencies();
        services.AddScoped<TempPasswordContext>();
        services.AddScoped<IAuthorizer, Authorizer>();
        services.AddScoped<IIdentityProvider, IdentityProvider>();
        services.AddScoped<IIdentityHelper, IdentityHelper>();

        services.AddHttpClient();
        services.AddScoped<ElasticSearchHelper>();
        services.AddScoped<ElasticSearchUserUpdater>();
        services.AddScoped<EventStoreUserUpdater>();
        services.AddScoped<MongoDbUserUpdater>();
        services.AddScoped<IUserUpdater, UserUpdater>();

        return services;
    }

    private static IServiceCollection AddDomainDependencies(this IServiceCollection services)
    {
        services.AddScoped<EventStoreClientSettingsFactory>();
        services.AddScoped<IMongoClientFactory, MongoClientFactory>();
        services.AddScoped<IEventStoreClientFactory, EventStoreClientFactory>();
        services.AddScoped<IEventStoreUserManagementClientFactory, EventStoreUserManagementClientFactory>();
        services.AddScoped<IEventStorePersistentSubscriptionsClientFactory, EventStorePersistentSubscriptionsClientFactory>();
        services.AddScoped<IServiceIdentityProvider, ServiceIdentityProvider>();

        return services;
    }
}
