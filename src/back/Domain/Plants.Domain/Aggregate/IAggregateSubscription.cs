using Plants.Shared.Model;

namespace Plants.Domain.Aggregate;

/// <typeparam name="TIn">Aggregate in which event is being transposed</typeparam>
/// <typeparam name="TOut">Aggregate from which event has been send</typeparam>
public interface IAggregateSubscription<TIn, TOut> where TIn : AggregateBase where TOut : AggregateBase
{
    IEnumerable<EventSubscriptionBase<TIn, TOut>> Subscriptions { get; }
}

public abstract record EventSubscriptionBase<TIn, TOur>(OneOf<FilteredEvents, AllEvents> EventFilter);

public record EventSubscription<TIn, TOut>
    (
        OneOf<FilteredEvents, AllEvents> EventFilter,
        AggregateLoadingTranspose<TIn> TransposeEvent
    ) : EventSubscriptionBase<TIn, TOut>(EventFilter)
    where TIn : AggregateBase where TOut : AggregateBase;

public record EventSubscription<TIn, TOut, TEvent>
    (
        AggregateLoadingTranspose<TIn, TEvent> TransposeEvent
    ) : EventSubscriptionBase<TIn, TOut>(new FilteredEvents(new[] { typeof(TEvent).Name }))
    where TIn : AggregateBase where TOut : AggregateBase where TEvent : Event;


/// <summary>
/// you should move event version by additional 1 (for parent command)
/// </summary>
public record AggregateLoadingTranspose<TAggregate>
    (Func<Event, Guid> ExtractId, Func<IEnumerable<Event>, TAggregate, IEnumerable<Event>> Transpose)
    where TAggregate : AggregateBase;

public record AggregateLoadingTranspose<TAggregate, TEvent>
    (Func<TEvent, Guid> ExtractId, Func<IEnumerable<TEvent>, TAggregate, IEnumerable<Event>> Transpose)
    where TEvent : Event where TAggregate : AggregateBase;


public record FilteredEvents(string[] EventNames);
public record AllEvents();

public static class EventSubscriptionFactory
{
    public static EventSubscription<TIn, TOut, TEvent> CreateForwarded<TIn, TOut, TEvent>(Func<TEvent, Guid> extractId) where TEvent : Event where TIn : AggregateBase where TOut : AggregateBase =>
        CreateForwarded<TIn, TOut, TEvent>(extractId, (_, _) => Array.Empty<Event>());

    public static EventSubscription<TIn, TOut, TEvent> CreateForwarded<TIn, TOut, TEvent>(Func<TEvent, Guid> extractId, Func<TIn, IEnumerable<TEvent>, IEnumerable<Event>> additionalEvents) where TEvent : Event where TIn : AggregateBase where TOut : AggregateBase =>
       new EventSubscription<TIn, TOut, TEvent>(new(extractId, (events, agg) => events.Select(@event => agg.TransposeSubscribedEvent(@event)).Union(additionalEvents(agg, events))));
}