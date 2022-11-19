using Plants.Shared;

namespace Plants.Domain;

/// <typeparam name="TIn">Aggregate in which event is being transposed</typeparam>
/// <typeparam name="TOut">Aggregate from which event has been send</typeparam>
public interface IAggregateSubscription<TIn, TOut> where TIn : AggregateBase where TOut : AggregateBase
{
    static abstract IEnumerable<EventSubscription<TIn, TOut>> Subscriptions { get; }
}

public record EventSubscription<TIn, TOut>
    (
        OneOf<FilteredEvents, AllEvents> EventFilter,
        OneOf<SimpleTranspose, AggregateLoadingTranspose<TIn>> TransposeEvent
    ) where TIn : AggregateBase where TOut : AggregateBase;

public record SimpleTranspose(Func<Event, Event> Transpose);
public record AggregateLoadingTranspose<TAggregate>(Func<Event, Guid> ExtractId, Func<Event, TAggregate, Event> Transpose);

public record FilteredEvents(string[] EventNames);
public record AllEvents();
