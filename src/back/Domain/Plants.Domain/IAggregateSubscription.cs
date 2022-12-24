using Plants.Shared;

namespace Plants.Domain;

/// <typeparam name="TIn">Aggregate in which event is being transposed</typeparam>
/// <typeparam name="TOut">Aggregate from which event has been send</typeparam>
public interface IAggregateSubscription<TIn, TOut> where TIn : AggregateBase where TOut : AggregateBase
{
    IEnumerable<EventSubscription<TIn, TOut>> Subscriptions { get; }
}

public record EventSubscription<TIn, TOut>
    (
        OneOf<FilteredEvents, AllEvents> EventFilter,
        AggregateLoadingTranspose<TIn> TransposeEvent
    ) where TIn : AggregateBase where TOut : AggregateBase;

/// <summary>
/// you should move event version by additional 1 (for parent command)
/// </summary>
public record AggregateLoadingTranspose<TAggregate>(Func<Event, Guid> ExtractId, Func<IEnumerable<Event>, TAggregate, IEnumerable<Event>> Transpose);

public record FilteredEvents(string[] EventNames);
public record AllEvents();
