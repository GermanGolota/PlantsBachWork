namespace Plants.Domain;

/// <typeparam name="TIn">Aggregate in which event is being transposed</typeparam>
/// <typeparam name="TOut">Aggregate from which event has been send</typeparam>
public interface IAggregateSubscription<TIn, TOut> where TIn : AggregateBase where TOut : AggregateBase
{
    IEnumerable<EventSubscriptionBase<TIn, TOut>> Subscriptions { get; }
}

public abstract record EventSubscriptionBase<TIn, TOur>();

public record EventSubscription<TIn, TOut>
    (
        OneOf<FilteredEvents, AllEvents> EventFilter,
        AggregateLoadingTranspose<TIn> TransposeEvent
    ) : EventSubscriptionBase<TIn, TOut>
    where TIn : AggregateBase where TOut : AggregateBase;

public record EventSubscription<TIn, TOut, TEvent>
    (
        AggregateLoadingTranspose<TIn, TEvent> TransposeEvent
    ) : EventSubscriptionBase<TIn, TOut>
    where TIn : AggregateBase where TOut : AggregateBase where TEvent : Event;


/// <summary>
/// you should move event version by additional 1 (for parent command)
/// </summary>
public record AggregateLoadingTranspose<TAggregate>
    (Func<Event, Guid> ExtractId, Func<Event, TAggregate, Event> Transpose)
    where TAggregate : AggregateBase;

public record AggregateLoadingTranspose<TAggregate, TEvent>
    (Func<TEvent, Guid> ExtractId, Func<TEvent, TAggregate, Event> Transpose)
    where TEvent : Event where TAggregate : AggregateBase;


public record FilteredEvents(string[] EventNames);
public record AllEvents();
