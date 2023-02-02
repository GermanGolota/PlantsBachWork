using EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Plants.Domain.Infrastructure;

internal class EventSubscription : IEventSubscription
{
    private readonly IEventStorePersistentSubscriptionsClientFactory _clientFactory;
    private readonly AggregateHelper _aggregate;
    private readonly ILogger<EventSubscription> _logger;
    private readonly IServiceIdentityProvider _serviceIdentity;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SubscriptionConfig _options;

    public EventSubscription(IEventStorePersistentSubscriptionsClientFactory clientFactory,
        AggregateHelper aggregate, ILogger<EventSubscription> logger,
        IServiceIdentityProvider serviceIdentity, IServiceScopeFactory scopeFactory,
        IOptions<SubscriptionConfig> options)
    {
        _clientFactory = clientFactory;
        _aggregate = aggregate;
        _logger = logger;
        _serviceIdentity = serviceIdentity;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    private IEnumerable<(IServiceScope, PersistentSubscription)>? _subscriptions = null;

    public async Task StartAsync(CancellationToken token)
    {
        _serviceIdentity.SetServiceIdentity();

        var client = _clientFactory.Create();
        var settings = CreateSettings(_options);

        var subscriptions = new List<PersistentSubscription>();
        var scopes = new List<IServiceScope>();

        var subscriptionTasks = _aggregate.Aggregates.Firsts
            .Select(async aggregateName =>
            {
                await CreateSubscriptionAsync(client, settings, aggregateName, token);

                return await StartSubscriptionAsync(client, aggregateName, token);
            });

        _subscriptions = await Task.WhenAll(subscriptionTasks);
    }

    private async Task<(IServiceScope, PersistentSubscription)> StartSubscriptionAsync(EventStorePersistentSubscriptionsClient client, string aggregate, CancellationToken token)
    {
        var scope = _scopeFactory.CreateScope();
        var provider = scope.ServiceProvider;
        provider.GetRequiredService<IServiceIdentityProvider>().SetServiceIdentity();
        var aggregateSubscription = provider.GetRequiredService<AggregateEventSubscription>();
        aggregateSubscription.Initialize(aggregate);
        var subscription = await client.SubscribeToAllAsync(aggregate,
            aggregateSubscription.Process,
            HandleStop,
            cancellationToken: token);

        return (scope, subscription);
    }

    private static async Task CreateSubscriptionAsync(EventStorePersistentSubscriptionsClient client, PersistentSubscriptionSettings settings, string aggregate, CancellationToken token)
    {
        var filter = StreamFilter.Prefix(aggregate);
        try
        {
            await client.CreateToAllAsync(aggregate, filter, settings, cancellationToken: token);
        }
        catch (RpcException e)
        {
            if (e.StatusCode != StatusCode.AlreadyExists)
            {
                throw;
            }
        }
    }

    private PersistentSubscriptionSettings CreateSettings(SubscriptionConfig options) =>
        new PersistentSubscriptionSettings(
            messageTimeout: TimeSpan.FromSeconds(options.CommandProcessingTimeoutInSeconds),
            maxRetryCount: 0);

    private void HandleStop(PersistentSubscription subscription, SubscriptionDroppedReason dropReason, Exception? exception)
    {
        if (exception is not null)
        {
            _logger.LogWarning(exception, "Dropped the connection stating '{reason}'", dropReason.ToString());
        }
        else
        {
            _logger.LogInformation("Dropped the connection stating '{reson}'", dropReason.ToString());
        }
    }

    public void Stop()
    {
        if (_subscriptions is null)
        {
            throw new InvalidOperationException("Cannot stop service before starting it");
        }

        foreach (var (scope, sub) in _subscriptions)
        {
            scope.Dispose();
            sub.Dispose();
        }
    }
}
