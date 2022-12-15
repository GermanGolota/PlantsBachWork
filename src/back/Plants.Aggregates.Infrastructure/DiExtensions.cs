using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Plants.Aggregates.Infrastructure.Encryption;
using Plants.Aggregates.Infrastructure.Helper;
using Plants.Aggregates.Infrastructure.Services;
using Plants.Aggregates.Services;
using Plants.Infrastructure.Config;
using System.Net;
using System.Net.Security;

namespace Plants.Aggregates.Infrastructure;

public static class DiExtensions
{
    public static IServiceCollection AddAggregatesInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<SymmetricEncrypter>();

        services.AddTransient<IEmailer, Emailer>();
        services.AddScoped<IIdentityProvider, IdentityProvider>();

        services.AddTransient<UsersManager>(factory =>
        {
            var config = factory.GetRequiredService<IOptions<ConnectionConfig>>().Value;
            var configs = config.EventStoreConnection
                        .Split(';')
                        .Where(_ => String.IsNullOrEmpty(_) is false)
                        .Select(x => x.Split('='))
                        .ToDictionary(x => x[0], y => y[1]);
            var url = new Uri(configs["ConnectTo"]);
            var hostInfo = Dns.GetHostEntry(url.Host);
            var endPoint = new IPEndPoint(hostInfo.AddressList[0], 2113);
            return new UsersManager(new ConsoleLogger(), endPoint, TimeSpan.FromSeconds(10));
        });

        services.AddScoped<EventStoreUserUpdater>();
        services.AddScoped<MongoDbUserUpdater>();
        services.AddScoped<IUserUpdater, UserUpdater>();

        return services;
    }
}
