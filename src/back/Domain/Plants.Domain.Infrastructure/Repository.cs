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
        if (_aggregateHelper.Aggregates.TryGetFor(typeof(TAggregate), out var aggregateName))
        {
            var events = await _store.ReadEventsAsync(new(id, aggregateName));
            var desc = new AggregateDescription(id, aggregateName);
            var aggregate = _applyer.ApplyEvents(desc, events);
            return (TAggregate)aggregate;
        }
        else
        {
            throw new Exception($"Cannot find aggregate '{typeof(TAggregate)}'");
        }
    }
}
