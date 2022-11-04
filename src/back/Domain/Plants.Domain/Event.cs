namespace Plants.Domain;

public abstract record Event(Guid Id, AggregateDescription Aggregate, long EventNumber, Guid CommandId, DateTime Time, string Name);