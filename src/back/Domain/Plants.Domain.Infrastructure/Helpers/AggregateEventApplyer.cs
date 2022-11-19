﻿using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure.Helpers;

internal class AggregateEventApplyer
{
    private readonly CqrsHelper _cqrs;
    private readonly AggregateHelper _aggregateHelper;

    public AggregateEventApplyer(CqrsHelper cqrs, AggregateHelper aggregateHelper)
    {
        _cqrs = cqrs;
        _aggregateHelper = aggregateHelper;
    }

    public TAggregate ApplyEvents<TAggregate>(Guid aggregateId, IEnumerable<Event> events) where TAggregate : AggregateBase
    {
        var aggregateType = typeof(TAggregate);
        if (_aggregateHelper.AggregateCtors.TryGetValue(aggregateType, out var ctor))
        {
            var aggregate = (TAggregate)ctor.Invoke(new object[] { aggregateId });
            var handlerBase = typeof(IEventHandler<>);
            var bumpFunc = aggregateType.GetMethod(nameof(AggregateBase.BumpVersion));
            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                var handlerType = handlerBase.MakeGenericType(eventType);
                if (_cqrs.EventHandlers.TryGetValue(eventType, out var handlers))
                {
                    foreach (var handler in handlers.Where(x => x.DeclaringType == aggregateType))
                    {
                        handler.Invoke(aggregate, new object[] { @event });
                        bumpFunc!.Invoke(aggregate, null);
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