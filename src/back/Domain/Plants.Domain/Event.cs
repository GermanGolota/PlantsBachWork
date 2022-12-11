namespace Plants.Domain;

public abstract record Event(EventMetadata Metadata);
public sealed record EventMetadata(Guid Id, AggregateDescription Aggregate, long EventNumber, Guid CommandId, DateTime Time, string Name);
public record FailEvent(EventMetadata Metadata, string[] Reasons) : Event(Metadata);