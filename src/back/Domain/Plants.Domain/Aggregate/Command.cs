namespace Plants.Domain.Aggregate;

public abstract record Command(CommandMetadata Metadata);
public sealed record CommandMetadata(Guid Id, AggregateDescription Aggregate, DateTime Time, string Name, string UserName, AggregateDescription? InitialAggregate = null);