namespace Plants.Domain;

public record Event(Guid Id, AggregateDescription Aggregate, Guid CommandId, DateTime Time, string Name);