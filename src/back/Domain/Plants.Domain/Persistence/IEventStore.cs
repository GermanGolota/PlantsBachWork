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

public static class EventStoreExtensions
{
    //TODO: Add error handling here
    public static async Task AppendEventsAsync(this IEventStore store, IEnumerable<Event> events, ulong startingVersion, Command command)
    {
        ulong currentVersion = startingVersion;
        foreach (var @event in @events)
        {
            var @eventToPublish = @event.ChangeVersion(currentVersion);
            currentVersion = await store.AppendEventAsync(@eventToPublish);
        }
        var lastEvent = new CommandProcessedEvent(EventFactory.Shared.Create<CommandProcessedEvent>(command));
        await store.AppendEventAsync(lastEvent.ChangeVersion(currentVersion));
    }
}