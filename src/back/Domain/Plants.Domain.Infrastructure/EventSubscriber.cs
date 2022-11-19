using Microsoft.Extensions.DependencyInjection;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure;

internal class EventSubscriber
{
    private readonly RepositoryCaller _caller;
    private readonly CqrsHelper _cqrs;
    private readonly AggregateEventApplyer _eventApplyer;
    private readonly IServiceProvider _services;

    public EventSubscriber(RepositoryCaller caller, CqrsHelper cqrs, AggregateEventApplyer eventApplyer, IServiceProvider services)
    {
        _caller = caller;
        _cqrs = cqrs;
        _eventApplyer = eventApplyer;
        _services = services;
    }

    public async Task UpdateAggregateAsync(AggregateDescription desc, IEnumerable<Event> newEvents)
    {
        var aggregate = await _caller.LoadAsync(desc);
        var newAggregate = _eventApplyer.ApplyEventsTo(aggregate, newEvents);
        if (newAggregate.Version is 0)
        {
            await _caller.CreateAsync(aggregate);
        }
        else
        {
            await _caller.UpdateAsync(aggregate);
        }
    }

    public async Task UpdateSubscribersAsync(AggregateDescription aggregate, List<Event> aggEvents)
    {
        //subs
        foreach (var (subscriberType, eventFilter) in _cqrs.EventSubscribers[aggregate.Name])
        {
            var eventsToHandle = eventFilter.Match(
                filter => aggEvents.Where(x => filter.EventNames.Contains(x.Metadata.Name)),
                all => aggEvents);
            if (eventsToHandle.Any())
            {
                var sub = (IEventSubscriber)_services.GetRequiredService(subscriberType);
                foreach (var @event in eventsToHandle)
                {
                    await sub.HandleAsync(@event);
                }
            }
        }
    }
}
