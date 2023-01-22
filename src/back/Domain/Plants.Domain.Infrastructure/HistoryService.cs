using Plants.Domain.Infrastructure.Extensions;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Infrastructure.Domain.Helpers;
using System.Linq;

namespace Plants.Domain.History;

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

    public async Task<HistoryModel> GetAsync(AggregateDescription desc, CancellationToken token)
    {
        var results = await _store.ReadEventsAsync(desc, token);
        var aggregate = _applyer.ConstructAggregate(desc);

        List<AggregateSnapshot> snapshots = new();

        foreach (var result in results)
        {
            aggregate = _applyer.ApplyEventsTo(aggregate, new[] { result });
            snapshots.Add(new(
                result.Command.Metadata.Time,
                new ObjectWithMetadata<AggregateMetadata>(RemoveMetadata(aggregate), aggregate.Metadata),
                new CommandSnapshot(RemoveMetadata(result.Command), result.Command.Metadata, result.Command.Metadata.InitialAggregate is null),
                result.Events
                    .Where(_ => _ is not CommandProcessedEvent)
                    .Select(_ => new ObjectWithMetadata<EventMetadata>(RemoveMetadata(_), _.Metadata))
                    .ToList()
                ));
        }
        var refences = aggregate.Metadata.Referenced
            .Select(reference => new RelatedAggregate(reference.Name, reference.Id, _helper.ReferencedAggregates[desc.Name][reference.Name].Name))
            .ToList();
        return new(snapshots, refences);
    }

    private static object RemoveMetadata(AggregateBase aggregate) =>
        aggregate.RemoveProperty(nameof(AggregateBase.Metadata));

    private static object RemoveMetadata(Command command) =>
        command.RemoveProperty(nameof(Command.Metadata));

    private static object RemoveMetadata(Event @event) =>
        @event.RemoveProperty(nameof(Event.Metadata));

    private static CommandHandlingResult CleanUp(CommandHandlingResult result) =>
        new(result.Command, result.Events.Where(_ => _ is not CommandProcessedEvent));
}
