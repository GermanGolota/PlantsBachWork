using EventStore.Client;
using Microsoft.Extensions.Logging;

namespace Plants.Domain.Infrastructure;

internal class AggregateEventSubscription
{
    public AggregateEventSubscription(EventStoreConverter converter,
        ILogger<AggregateEventSubscription> logger,
        EventSubscriptionProcessor processor,
        IEventSubscriptionState state)
    {
        _converter = converter;
        _logger = logger;
        _processor = processor;
        _state = state;
    }

    private readonly EventStoreConverter _converter;
    private readonly ILogger<AggregateEventSubscription> _logger;
    private readonly EventSubscriptionProcessor _processor;
    private readonly IEventSubscriptionState _state;

    public void Initialize(string aggregateName) =>
        _aggregateName = aggregateName;

    private string _aggregateName = null!;

    public async Task Process(PersistentSubscription subscription, ResolvedEvent @event, int? retryCount, CancellationToken token)
    {
        if (retryCount is not null && retryCount is not 0)
        {
            _logger.LogWarning("Retrying subscription processing for the '{retryCount}' count", retryCount);
        }

        var result = _converter.Convert(@event);
        var aggregateId = result.Match(_ => _.Metadata.Aggregate.Id, _ => _.Metadata.Aggregate.Id);
        var subscriptionState = _state.GetState(new(aggregateId, _aggregateName));
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
        var subscriptionState = _state.GetState(new(aggregateId, _aggregateName));
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
            _state.RemoveState(new(aggregateId, _aggregateName));
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
