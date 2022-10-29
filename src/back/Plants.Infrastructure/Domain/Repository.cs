﻿using Plants.Domain;
using Plants.Domain.Persistence;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Infrastructure.Domain;

public class Repository<TAggregate> : IRepository<TAggregate> where TAggregate : AggregateBase
{
    private readonly IEventStore _store;
    private readonly AggregateHelper _helper;
    private readonly CQRSHelper _cqrs;

    public Repository(IEventStore store, AggregateHelper helper, CQRSHelper cqrs)
    {
        _store = store;
        _helper = helper;
        _cqrs = cqrs;
    }

    public async Task<TAggregate> GetByIdAsync(Guid id)
    {
        var aggregateType = typeof(TAggregate);
        if (_helper.AggregateCtors.TryGetValue(aggregateType, out var ctor))
        {
            var aggregate = (TAggregate)ctor.Invoke(new object[] { id });
            var events = await _store.ReadEventsAsync(id);
            var handlerBase = typeof(IEventHandler<>);
            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                var handlerType = handlerBase.MakeGenericType(eventType);
                if (_cqrs.EventHandlers.TryGetValue(eventType, out var handlers))
                {
                    foreach (var handler in handlers.Where(x => x.DeclaringType == aggregateType))
                    {
                        handler.Invoke(aggregate, new object[] { @event });
                    }
                }
            }
            return aggregate;
        }
        else
        {
            throw new Exception($"Cannot construct '{aggregateType}'");
        }
    }
}
