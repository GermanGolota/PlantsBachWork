using Plants.Domain.Persistence;
using Plants.Domain.Projection;

namespace Plants.Domain.Services;

public class EventSubscriberHelper<TAggregate> where TAggregate : AggregateBase
{
    private readonly IRepository<TAggregate> _repository;
    private readonly IProjectionRepository<TAggregate> _projection;

    public EventSubscriberHelper(
        IRepository<TAggregate> repository,
        IProjectionRepository<TAggregate> projection)
    {
        _repository = repository;
        _projection = projection;
    }

    public async Task UpdateAsync<TEvent>(TEvent @event, Func<TEvent, Guid> extractAggregateId, Func<TEvent, TAggregate, TAggregate> process) where TEvent : Event
    {
        var aggregateId = extractAggregateId(@event);
        var aggregate = await _repository.GetByIdAsync(aggregateId);
        process(@event, aggregate);
        aggregate.BumpVersion();
        await _projection.InsertOrUpdateAsync(aggregate);
    }
}
