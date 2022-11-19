using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Persistence;

namespace Plants.Domain.Infrastructure;

internal class Repository<TAggregate> : IRepository<TAggregate> where TAggregate : AggregateBase
{
    private readonly IEventStore _store;
    private readonly AggregateEventApplyer _applyer;

    public Repository(IEventStore store, AggregateEventApplyer applyer)
    {
        _store = store;
        _applyer = applyer;
    }

    public async Task<TAggregate> GetByIdAsync(Guid id)
    {
        var events = await _store.ReadEventsAsync(id);
        return _applyer.ApplyEvents<TAggregate>(id, events);
    }
}
