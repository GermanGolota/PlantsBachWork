namespace Plants.Domain;

public interface IHistoryService
{
    Task<HistoryModel> GetAsync(AggregateDescription aggregate, CancellationToken token);
}

public record HistoryModel(List<AggregateSnapshot> Snapshots, List<RelatedAggregate> Related);

public record RelatedAggregate(string Name, Guid Id, string Role);

public record AggregateSnapshot(
    DateTime Time,
    ObjectWithMetadata<AggregateMetadata> Aggregate,
    CommandSnapshot LastCommand,
    List<ObjectWithMetadata<EventMetadata>> Events
    );

public record ObjectWithMetadata<TMeta>(object Payload, TMeta Metadata);

public record CommandSnapshot(object Payload, CommandMetadata Metadata, bool IsLocal);