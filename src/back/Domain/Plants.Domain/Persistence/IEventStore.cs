namespace Plants.Domain;

public interface IEventStore
{
    Task<IEnumerable<CommandHandlingResult>> ReadEventsAsync(AggregateDescription aggregate, DateTime? asOf = null, CancellationToken token = default);

    Task<IEnumerable<(string AggregateName, List<Guid> Ids)>> GetStreamsAsync(CancellationToken token);

    /// <returns>Next expected version</returns>
    Task<ulong> AppendEventAsync(Event @event, CancellationToken token = default);

    /// <returns>Next expected version</returns>
    Task<ulong> AppendCommandAsync(Command command, ulong version, CancellationToken token = default);
}

public record CommandHandlingResult(Command Command, IEnumerable<Event> Events);

public static class EventStoreExtensions
{
    //TODO: Add error handling here
    public static async Task AppendEventsAsync(this IEventStore store, IEnumerable<Event> events, ulong startingVersion, Command command, CancellationToken token = default)
    {
        ulong currentVersion = startingVersion;
        foreach (var @event in @events)
        {
            var @eventToPublish = @event.ChangeVersion(currentVersion);
            currentVersion = await store.AppendEventAsync(@eventToPublish, token);
        }
        var lastEvent = new CommandProcessedEvent(EventFactory.Shared.Create<CommandProcessedEvent>(command));
        await store.AppendEventAsync(lastEvent.ChangeVersion(currentVersion), token);
    }
}