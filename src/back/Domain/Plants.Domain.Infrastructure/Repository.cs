using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Persistence;
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

    public async Task<TAggregate> GetByIdAsync(Guid id)
    {
        var events = await _store.ReadEventsAsync(id);
        if (_aggregateHelper.Aggregates.TryGetFor(typeof(TAggregate), out var aggregateName))
        {
            var desc = new AggregateDescription(id, aggregateName);
            return (TAggregate)_applyer.ApplyEvents(desc, events.SelectMany(x => x.Events));
        }
        else
        {
            throw new Exception($"Cannot find aggregate '{typeof(TAggregate)}'");
        }
    }
}
