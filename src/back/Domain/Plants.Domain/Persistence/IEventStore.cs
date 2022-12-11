namespace Plants.Domain.Persistence;

public interface IEventStore
{
    Task<IEnumerable<CommandHandlingResult>> ReadEventsAsync(Guid id);

    /// <returns>Next expected version</returns>
    Task<long> AppendEventAsync(Event @event);

    /// <returns>Next expected version</returns>
    Task<long> AppendCommandAsync(Command command, long version);
}

public record CommandHandlingResult(Command Command, IEnumerable<Event> Events);