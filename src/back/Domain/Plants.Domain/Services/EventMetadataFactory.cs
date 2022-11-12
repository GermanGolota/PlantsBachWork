namespace Plants.Domain.Services;

public sealed class EventMetadataFactory
{
    private readonly IDateTimeProvider _dateTime;

    public EventMetadataFactory(IDateTimeProvider dateTime)
    {
        _dateTime = dateTime;
    }

    public EventMetadata Create<T>(Command command, long eventNumber) where T : Event
    {
        var name = typeof(T).Name.Replace("Event", "");
        return new EventMetadata(Guid.NewGuid(), command.Metadata.Aggregate, eventNumber, command.Metadata.Id, _dateTime.UtcNow, name);
    }
}

public static class EventFactory
{
    private static Lazy<EventMetadataFactory> _factory = new Lazy<EventMetadataFactory>(() => new EventMetadataFactory(new DateTimeProvider()));
    public static EventMetadataFactory Shared => _factory.Value;
}