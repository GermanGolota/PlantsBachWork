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
        //would make sense for times in which we would load projection and apply events to it
        //var newAggregate = _eventApplyer.ApplyEventsTo(aggregate, newEvents);
        await _caller.InsertOrUpdateProjectionAsync(aggregate);
    }

    public async Task UpdateSubscribersAsync(AggregateDescription aggregate, List<Event> aggEvents)
    {
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
