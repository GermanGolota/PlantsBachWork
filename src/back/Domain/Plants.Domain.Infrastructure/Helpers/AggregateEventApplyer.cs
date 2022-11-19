using Plants.Infrastructure.Domain.Helpers;

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

    public AggregateBase ApplyEvents(AggregateDescription desc, IEnumerable<Event> events)
    {
        if (_aggregateHelper.Aggregates.TryGetFor(desc.Name, out var aggregateType))
        {
            if (_aggregateHelper.AggregateCtors.TryGetValue(aggregateType, out var ctor))
            {
                var aggregate = (AggregateBase)ctor.Invoke(new object[] { desc.Id });
                return ApplyEventsTo(aggregate, events);
            }
            else
            {
                throw new Exception($"Cannot construct '{aggregateType}'");
            }
        }
        else
        {
            throw new Exception($"Cannot find '{desc.Name}'");
        }
    }

    public AggregateBase ApplyEventsTo(AggregateBase aggregate, IEnumerable<Event> events)
    {
        if (_aggregateHelper.Aggregates.TryGetFor(aggregate.Name, out var aggregateType))
        {
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
            throw new Exception($"Cannot find '{aggregate.Name}'");
        }
    }
}