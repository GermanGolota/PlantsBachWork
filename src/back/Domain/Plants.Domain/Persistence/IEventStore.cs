namespace Plants.Domain.Persistence;

public interface IEventStore
{
    Task<IEnumerable<(Command Command, IEnumerable<Event> Events)>> ReadEventsAsync(Guid id);

    /// <returns>Next expected version</returns>
    Task<long> AppendEventAsync(Event @event);

    /// <returns>Next expected version</returns>
    Task<long> AppendCommandAsync(Command command, long version);
}
