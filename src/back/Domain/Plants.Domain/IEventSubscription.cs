using Plants.Shared;

namespace Plants.Domain;

public interface IEventSubscriber
{
    string Aggregate { get; }
    OneOf<FilteredEvents, AllEvents> Events { get; }
    Task HandleAsync(Event @event);
}

public record FilteredEvents(string[] EventNames);
public record AllEvents();
