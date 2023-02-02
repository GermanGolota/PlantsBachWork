using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Plants.Domain.Infrastructure;

public static class HealthChecksBuilderExtensions
{
    public static IHealthChecksBuilder AddDomainHealthChecks(this IHealthChecksBuilder builder, IConfiguration configuration)
    {
        var connection = configuration.GetSection(ConnectionConfig.Section).Get<ConnectionConfig>()!;
        var elasticCreds = connection.GetCreds(_ => _.ElasticSearch);
        var mongoCreds = connection.GetCreds(_ => _.MongoDb);
        var eventStoreCreds = connection.GetCreds(_ => _.EventStore);
        builder
            .AddElasticsearch(opt =>
            {
                opt.UseServer(connection.ElasticSearch.Template)
                   .UseBasicAuthentication(elasticCreds.Username, elasticCreds.Password);
            })
            .AddEventStore(connection.EventStore.Template, eventStoreCreds.Username, eventStoreCreds.Password)
            .AddMongoDb(connection.MongoDb.Template.Format(mongoCreds.Username, mongoCreds.Password));

        return builder;
    }

    /// <summary>
    /// Add a health check for EventStore services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The EventStore connection string to be used.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'eventstore' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    private static IHealthChecksBuilder AddEventStore(
        this IHealthChecksBuilder builder,
        string connectionString,
        string login,
        string password,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        builder.Services.AddSingleton(sp => new EventStoreHealthCheck(connectionString, login, password));
        return builder.Add(new HealthCheckRegistration(
            name ?? "eventstore",
            sp => sp.GetRequiredService<EventStoreHealthCheck>(),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Checks whether a gRPC connection can be made to EventStore services using the supplied connection string.
    /// </summary>
    private class EventStoreHealthCheck : IHealthCheck, IDisposable
    {
        private readonly EventStoreClient _client;

        public EventStoreHealthCheck(string connectionString, string login, string password)
        {
            ArgumentNullException.ThrowIfNull(connectionString);
            var settings = EventStoreClientSettings.Create(connectionString);
            if (login is not null && password is not null)
            {
                settings.DefaultCredentials = new(login, password);
            }
            _client = new EventStoreClient(settings);
        }

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var subscription = await _client.SubscribeToAllAsync(
                    FromAll.End,
                    eventAppeared: (_, _, _) => Task.CompletedTask,
                    cancellationToken: cancellationToken);

                return new HealthCheckResult(HealthStatus.Healthy);
            }
            catch (Exception exception)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: exception);
            }
        }

        public void Dispose() => _client.Dispose();
    }

}
