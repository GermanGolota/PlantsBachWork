using EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Infrastructure.Services;
using Plants.Infrastructure.Domain.Helpers;
using System.Collections.Concurrent;

namespace Plants.Domain.Infrastructure;

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

internal class AggregateEventSubscription
{
    public AggregateEventSubscription(EventStoreConverter converter,
        ILogger<AggregateEventSubscription> logger,
        EventSubscriptionProcessor processor)
    {
        _converter = converter;
        _logger = logger;
        _processor = processor;
    }

    #region Service
    private readonly EventStoreConverter _converter;
    private readonly ILogger<AggregateEventSubscription> _logger;
    private readonly EventSubscriptionProcessor _processor;
    #endregion

    #region State
    private ConcurrentDictionary<Guid, AggregateSubscriptionState> _aggregateStates = new();
    #endregion

    public async Task Process(PersistentSubscription subscription, ResolvedEvent @event, int? retryCount, CancellationToken token)
    {
        if (retryCount is not null && retryCount is not 0)
        {
            _logger.LogWarning("Retrying subscription processing for the '{retryCount}' count", retryCount);
        }

        var result = _converter.Convert(@event);
        var aggregateId = result.Match(_ => _.Metadata.Aggregate.Id, _ => _.Metadata.Aggregate.Id);
        var subscriptionState = _aggregateStates.GetOrAdd(aggregateId, _ => (new()));
        subscriptionState.EventIds.Add(@event.Event.EventId);
        result.Match(_ =>
        {
            subscriptionState.Events.Add(_);
        },
        command =>
        {
            subscriptionState.Command = command;
        });

        await TryProcessCommand(subscription, aggregateId, token);
    }

    private async Task TryProcessCommand(PersistentSubscription subscription, Guid aggregateId, CancellationToken cancellationToken)
    {
        var subscriptionState = _aggregateStates.GetOrAdd(aggregateId, _ => (new()));
        if (subscriptionState.Command is not null && subscriptionState.Events.Any(_ => _ is CommandProcessedEvent))
        {
            var command = subscriptionState.Command!;
            _logger.LogInformation("Processing subscription for '{aggName}'-'{aggId}' with command '{cmdName}'-'{cmdId}'", subscription.SubscriptionId, aggregateId, command.Metadata.Name, command.Metadata.Id);
            try
            {
                await _processor.ProcessCommandAsync(command, subscriptionState.Events, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process command from subscription for '{aggName}'-'{aggId}' with command '{cmdName}'-'{cmdId}'", subscription.SubscriptionId, aggregateId, command.Metadata.Name, command.Metadata.Id);
                //TODO: add dead letter queue here
            }
            await subscription.Ack(subscriptionState.EventIds);
            if (_aggregateStates.TryRemove(aggregateId, out var _) is false)
            {
                _logger.LogWarning("Failed to remove aggregate events from subscriber cache for '{aggName}'-'{aggId}' with command '{cmdName}'-'{cmdId}''", subscription.SubscriptionId, aggregateId, command.Metadata.Name, command.Metadata.Id);
            }
            _logger.LogInformation("Processed command from subscription for '{aggName}'-'{aggId}' with command '{cmdName}'-'{cmdId}'", subscription.SubscriptionId, aggregateId, command.Metadata.Name, command.Metadata.Id);
        }
    }

    private class AggregateSubscriptionState
    {
        public List<Uuid> EventIds { get; set; } = new();
        public Command? Command { get; set; } = null;
        public List<Event> Events { get; set; } = new();
    }

}