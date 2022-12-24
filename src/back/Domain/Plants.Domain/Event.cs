namespace Plants.Domain;

public abstract record Event(EventMetadata Metadata);
public sealed record EventMetadata(Guid Id, AggregateDescription Aggregate, ulong EventNumber, Guid CommandId, DateTime Time, string Name);
public record FailEvent(EventMetadata Metadata, string[] Reasons, bool IsException) : Event(Metadata);