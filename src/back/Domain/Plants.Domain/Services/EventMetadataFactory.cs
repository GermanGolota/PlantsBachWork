namespace Plants.Domain.Services;

public sealed class EventMetadataFactory
{
    private readonly IDateTimeProvider _dateTime;

    public EventMetadataFactory(IDateTimeProvider dateTime)
    {
        _dateTime = dateTime;
    }

    public EventMetadata Create<T>(AggregateDescription aggregate, long eventNumber, Guid commandId) where T : Event
    {
        var name = typeof(T).Name.Replace("Event", "");
        return new EventMetadata(Guid.NewGuid(), aggregate, eventNumber, commandId, _dateTime.UtcNow, name);
    }
}
