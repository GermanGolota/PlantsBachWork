using System.Reflection;

namespace Plants.Domain.Infrastructure;

internal class HistoryService : IHistoryService
{
    private readonly IEventStore _store;
    private readonly AggregateEventApplyer _applyer;
    private readonly AggregateHelper _helper;

    public HistoryService(IEventStore store, AggregateEventApplyer applyer, AggregateHelper helper)
    {
        _store = store;
        _applyer = applyer;
        _helper = helper;
    }

    public async Task<HistoryModel> GetAsync(AggregateDescription desc, OrderType order, DateTime? asOf = null, CancellationToken token = default)
    {
        var results = await _store.ReadEventsAsync(desc, asOf, token);
        var aggregate = _applyer.ConstructAggregate(desc);

        List<AggregateSnapshot> snapshots = new();

        foreach (var result in results)
        {
            aggregate = _applyer.ApplyEventsTo(aggregate, new[] { result });
            snapshots.Add(new(
                result.Command.Metadata.Time,
                new ObjectWithMetadata<AggregateMetadata>(RemoveMetadataAndRelatedAggregates(aggregate), aggregate.Metadata),
                new CommandSnapshot(RemoveMetadata(result.Command), result.Command.Metadata, result.Command.Metadata.InitialAggregate is null),
                result.Events
                    .Where(_ => _ is not CommandProcessedEvent)
                    .Select(_ => new ObjectWithMetadata<EventMetadata>(RemoveMetadata(_), _.Metadata))
                    .ToList(),
                aggregate.Metadata.Referenced.Select(reference => GetReferenced(desc, reference)).ToList()
                ));
        }
        switch (order)
        {
            case OrderType.Historical:
                snapshots = snapshots.OrderBy(_ => _.Time).ToList();
                break;
            case OrderType.ReverseHistorical:
                snapshots = snapshots.OrderByDescending(_ => _.Time).ToList();
                break;
        }
        return new(snapshots);
    }

    private RelatedAggregate GetReferenced(AggregateDescription desc, AggregateDescription reference) =>
        new(reference.Name, reference.Id, RelatedProps(desc)[reference.Name].Name);

    private IReadOnlyDictionary<string, PropertyInfo> RelatedProps(AggregateDescription desc) =>
        _helper.ReferencedAggregates[desc.Name];

    private object RemoveMetadataAndRelatedAggregates(AggregateBase aggregate) =>
        aggregate.RemoveProperties(
            RelatedProps(aggregate.GetDescription())
            .Select(_ => _.Value.Name)
            .Append(nameof(AggregateBase.Metadata))
            .ToArray()
            );

    private static object RemoveMetadata(Command command) =>
        command.RemoveProperties(nameof(Command.Metadata));

    private static object RemoveMetadata(Event @event) =>
        @event.RemoveProperties(nameof(Event.Metadata));

    private static CommandHandlingResult CleanUp(CommandHandlingResult result) =>
        new(result.Command, result.Events.Where(_ => _ is not CommandProcessedEvent));
}
