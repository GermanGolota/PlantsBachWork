using EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plants.Domain.Infrastructure.Services;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure.Subscription;

internal class EventSubscription : IEventSubscription
{
    private readonly IEventStorePersistentSubscriptionsClientFactory _clientFactory;
    private readonly AggregateHelper _aggregate;
    private readonly ILogger<EventSubscription> _logger;
    private readonly IServiceIdentityProvider _serviceIdentity;
    private readonly IServiceScopeFactory _scopeFactory;

    public EventSubscription(IEventStorePersistentSubscriptionsClientFactory clientFactory,
        AggregateHelper aggregate, ILogger<EventSubscription> logger,
        IServiceIdentityProvider serviceIdentity, IServiceScopeFactory scopeFactory)
    {
        _clientFactory = clientFactory;
        _aggregate = aggregate;
        _logger = logger;
        _serviceIdentity = serviceIdentity;
        _scopeFactory = scopeFactory;
    }

    private IEnumerable<PersistentSubscription>? _subscriptions = null;
    private IEnumerable<IServiceScope>? _scopes = null;

    public async Task StartAsync(CancellationToken token)
    {
        _serviceIdentity.SetServiceIdentity();

        var client = _clientFactory.Create();
        var settings = new PersistentSubscriptionSettings();
        var subscriptions = new List<PersistentSubscription>();
        var scopes = new List<IServiceScope>();
        foreach (var (aggregate, _) in _aggregate.Aggregates)
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
            var scope = _scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;
            provider.GetRequiredService<IServiceIdentityProvider>().SetServiceIdentity();
            var aggregateSubscription = provider.GetRequiredService<AggregateEventSubscription>();
            aggregateSubscription.Initialize(aggregate);
            var subscription = await client.SubscribeToAllAsync(aggregate,
                aggregateSubscription.Process,
                HandleStop,
                cancellationToken: token);
            scopes.Add(scope);
            subscriptions.Add(subscription);
        }

        _scopes = scopes;
        _subscriptions = subscriptions;
    }

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

        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }

        foreach (var scope in _scopes!)
        {
            scope.Dispose();
        }
    }
}
