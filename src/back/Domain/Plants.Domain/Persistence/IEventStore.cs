namespace Plants.Domain.Persistence;

public interface IEventStore
{
    Task<IEnumerable<CommandHandlingResult>> ReadEventsAsync(AggregateDescription aggregate);

    /// <returns>Next expected version</returns>
    Task<ulong> AppendEventAsync(Event @event);

    /// <returns>Next expected version</returns>
    Task<ulong> AppendCommandAsync(Command command, ulong version);
}

public record CommandHandlingResult(Command Command, IEnumerable<Event> Events);