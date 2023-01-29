namespace Plants.Domain;

public interface IHistoryService
{
    Task<HistoryModel> GetAsync(AggregateDescription aggregate, OrderType order, CancellationToken token);
}

public enum OrderType
{
    Historical = 0,
    ReverseHistorical = 1
}

public record HistoryModel(List<AggregateSnapshot> Snapshots);

public record RelatedAggregate(string Name, Guid Id, string Role);

public record AggregateSnapshot(
    DateTime Time,
    ObjectWithMetadata<AggregateMetadata> Aggregate,
    CommandSnapshot LastCommand,
    List<ObjectWithMetadata<EventMetadata>> Events, 
    List<RelatedAggregate> Related
    );

public record ObjectWithMetadata<TMeta>(object Payload, TMeta Metadata);

public record CommandSnapshot(object Payload, CommandMetadata Metadata, bool IsLocal);