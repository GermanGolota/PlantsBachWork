namespace Plants.Domain;

public record Command(Guid Id, AggregateDescription Aggregate, DateTime Time, string Name, string UserName);