namespace Plants.Aggregates;

public record GroupSelectedEvent(EventMetadata Metadata, string GroupName) : Event(Metadata);
