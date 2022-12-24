using EventStore.Client;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plants.Aggregates.Infrastructure.Helper;
using Plants.Aggregates.Infrastructure.Services;
using Plants.Aggregates.Services;
using Plants.Aggregates.Users;
using Plants.Domain.Infrastructure.Config;
using Plants.Services.Infrastructure.Encryption;

namespace Plants.Aggregates.Infrastructure;

public static class DiExtensions
{
    public static IServiceCollection AddAggregatesInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<TempPasswordContext>();
        services.AddScoped<IAuthorizer, Authorizer>();
        services.AddScoped<IIdentityProvider, IdentityProvider>();
        services.AddScoped(factory =>
        {
            var options = factory.GetRequiredService<IOptions<ConnectionConfig>>().Value;
            var settings = EventStoreClientSettings.Create(options.EventStoreConnection);
            var identity = factory.GetRequiredService<IIdentityProvider>().Identity;
            if (identity is not null)
            {
                var symmetric = factory.GetRequiredService<SymmetricEncrypter>();
                settings.DefaultCredentials = new UserCredentials(identity.UserName, symmetric.Decrypt(identity.Hash));
            }

            settings.DefaultDeadline = TimeSpan.FromSeconds(options.EventStoreTimeoutInSeconds);
            settings.LoggerFactory ??= factory.GetService<ILoggerFactory>();
            settings.Interceptors ??= factory.GetServices<Interceptor>();
            return settings;
        });
        services.AddScoped(factory => new EventStoreUserManagementClient(factory.GetRequiredService<EventStoreClientSettings>()));

        services.AddScoped<EventStoreUserUpdater>();
        services.AddScoped<MongoDbUserUpdater>();
        services.AddScoped<IUserUpdater, UserUpdater>();

        return services;
    }
}
