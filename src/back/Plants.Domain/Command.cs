namespace Plants.Domain;

public abstract record Command(Guid Id, AggregateDescription Aggregate, DateTime Time, string Name, string UserName);