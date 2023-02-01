using Plants.Domain.Infrastructure.Helpers;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure;

internal class Repository<TAggregate> : IRepository<TAggregate> where TAggregate : AggregateBase
{
    private readonly IEventStore _store;
    private readonly AggregateEventApplyer _applyer;
    private readonly AggregateHelper _aggregateHelper;

    public Repository(IEventStore store, AggregateEventApplyer applyer, AggregateHelper aggregateHelper)
    {
        _store = store;
        _applyer = applyer;
        _aggregateHelper = aggregateHelper;
    }

    public async Task<TAggregate> GetByIdAsync(Guid id, DateTime? asOf = null, CancellationToken token = default)
    {
        if (_aggregateHelper.Aggregates.TryGetFor(typeof(TAggregate), out var aggregateName))
        {
            var desc = new AggregateDescription(id, aggregateName);
            var aggregate = await LoadAggregate(desc, asOf, token);
            return (TAggregate)aggregate;
        }
        else
        {
            throw new Exception($"Cannot find aggregate '{typeof(TAggregate)}'");
        }
    }

    private async Task<AggregateBase> LoadAggregate(AggregateDescription desc, DateTime? asOf, CancellationToken token = default)
    {
        var events = await _store.ReadEventsAsync(desc, asOf, token);
        var aggregate = _applyer.ApplyEvents(desc, events);
        var referencedFields = _aggregateHelper.ReferencedAggregates[desc.Name];
        foreach (var reference in aggregate.Metadata.Referenced)
        {
            var field = referencedFields[reference.Name];
            var value = await LoadAggregate(reference, asOf, token);
            field.SetValue(aggregate, value);
        }
        return aggregate;
    }
}
