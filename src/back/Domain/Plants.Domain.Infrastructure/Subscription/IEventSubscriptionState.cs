using EventStore.Client;
using Plants.Domain.Aggregate;

namespace Plants.Domain.Infrastructure.Subscription;

internal interface IEventSubscriptionState
{
    AggregateSubscriptionState GetState(AggregateDescription description);
    void RemoveState(AggregateDescription description);
}

internal class AggregateSubscriptionState
{
    public List<Uuid> EventIds { get; set; } = new();
    public Command? Command { get; set; } = null;
    public List<Event> Events { get; set; } = new();
}