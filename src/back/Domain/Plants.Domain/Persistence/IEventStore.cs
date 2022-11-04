namespace Plants.Domain.Persistence;

public interface IEventStore
{
    Task<IEnumerable<Event>> ReadEventsAsync(Guid id);

    /// <returns>Next expected version</returns>
    Task<long> AppendEventAsync(Event @event);
}
