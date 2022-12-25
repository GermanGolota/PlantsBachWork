using EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Infrastructure.Services;
using Plants.Domain.Services;
using Plants.Infrastructure.Domain.Helpers;
using Plants.Shared;
using System;
using System.Collections.Concurrent;

namespace Plants.Domain.Infrastructure;

internal class EventSubscriptionWorker : IEventSubscriptionWorker
{
    private readonly IEventStorePersistentSubscriptionsClientFactory _clientFactory;
    private readonly AggregateHelper _aggregate;
    private readonly ILogger<EventSubscriptionWorker> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly EventSubscriber _subscriber;
    private readonly EventStoreConverter _converter;
    private readonly IIdentityProvider _identityProvider;
    private readonly IServiceIdentityProvider _serviceIdentity;

    public EventSubscriptionWorker(IEventStorePersistentSubscriptionsClientFactory clientFactory,
        AggregateHelper aggregate, ILogger<EventSubscriptionWorker> logger, IDateTimeProvider dateTime,
        EventSubscriber subscriber, EventStoreConverter converter, IIdentityProvider identityProvider, IServiceIdentityProvider serviceIdentity)
    {
        _clientFactory = clientFactory;
        _aggregate = aggregate;
        _logger = logger;
        _dateTime = dateTime;
        _subscriber = subscriber;
        _converter = converter;
        _identityProvider = identityProvider;
        _serviceIdentity = serviceIdentity;
    }

    private IEnumerable<PersistentSubscription>? _subscriptions = null;

    public async Task StartAsync(CancellationToken token)
    {
        var service = _serviceIdentity.ServiceIdentity;
        _identityProvider.UpdateIdentity(service);

        var client = _clientFactory.Create();
        var settings = new PersistentSubscriptionSettings();
        var subscriptions = new List<PersistentSubscription>();
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
            var subscription = await client.SubscribeToAllAsync(aggregate,
                Process(_subscriber, _logger),
                HandleStop);
            subscriptions.Add(subscription);
        }

        _subscriptions = subscriptions;
    }

    private Func<PersistentSubscription, ResolvedEvent, int?, CancellationToken, Task> Process(EventSubscriber subscriber, ILogger logger)
    {
        ConcurrentDictionary<Guid, AggregateSubscriptionState> aggregateStates = new();
        return async (subscription, resolved, retryCount, cancellationToken) =>
        {
            var result = _converter.Convert(resolved);
            var aggregateId = result.Match(_ => _.Metadata.Aggregate.Id, _ => _.Metadata.Aggregate.Id);
            var subscriptionState = aggregateStates.GetOrAdd(aggregateId, _ => new());
            subscriptionState.EventIds.Add(resolved.Event.EventId);
            await result.MatchAsync(async @event =>
            {
                subscriptionState.Events.Add(@event);
                if (@event is CommandProcessedEvent)
                {
                    var command = subscriptionState.Command!;
                    logger.LogInformation("Processing subscription for '{aggName}'-'{aggId}' with command '{cmdName}'-'{cmdId}'", subscription.SubscriptionId, aggregateId, command.Metadata.Name, command.Metadata.Id);
                    try
                    {
                        await subscriber.ProcessCommand(command, subscriptionState.Events);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to process command from subscription for '{aggName}'-'{aggId}' with command '{cmdName}'-'{cmdId}'", subscription.SubscriptionId, aggregateId, command.Metadata.Name, command.Metadata.Id);
                        //todo: add dead letter queue here
                    }
                    await subscription.Ack(subscriptionState.EventIds);
                    if (aggregateStates.TryRemove(aggregateId, out var _) is false)
                    {
                        logger.LogWarning("Failed to remove aggregate events from subscriber cache for '{aggName}'-'{aggId}' with command '{cmdName}'-'{cmdId}''", subscription.SubscriptionId, aggregateId, command.Metadata.Name, command.Metadata.Id);
                    }
                    logger.LogInformation("Processed command from subscription for '{aggName}'-'{aggId}' with command '{cmdName}'-'{cmdId}'", subscription.SubscriptionId, aggregateId, command.Metadata.Name, command.Metadata.Id);
                }
            },
            command =>
            {
                subscriptionState.Command = command;
                return Task.CompletedTask;
            });
        };
    }

    private class AggregateSubscriptionState
    {
        public List<Uuid> EventIds { get; set; } = new();
        public Command? Command { get; set; } = null;
        public List<Event> Events { get; set; } = new();
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

    public void Stop(CancellationToken token)
    {
        if (_subscriptions is null)
        {
            throw new InvalidOperationException("Cannot stop service before starting it");
        }

        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }
    }
}
