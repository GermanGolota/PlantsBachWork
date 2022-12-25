namespace Plants.Domain;

public abstract record Event(EventMetadata Metadata);
public sealed record EventMetadata(Guid Id, AggregateDescription Aggregate, Guid CommandId, DateTime Time, string Name, ulong EventNumber = UInt64.MaxValue);
public record FailEvent(EventMetadata Metadata, string[] Reasons, bool IsException) : Event(Metadata);
public record CommandProcessedEvent(EventMetadata Metadata) : Event(Metadata);