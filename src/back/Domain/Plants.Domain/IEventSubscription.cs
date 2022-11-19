using Plants.Shared;

namespace Plants.Domain;

public interface IEventSubscriber
{
    static abstract string Aggregate { get; }
    static abstract OneOf<FilteredEvents, AllEvents> Events { get; }
    Task HandleAsync(Event @event);
}

public record FilteredEvents(string[] EventNames);
public record AllEvents();
