using Microsoft.Extensions.DependencyInjection;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Persistence;
using Plants.Infrastructure.Domain.Helpers;
using Plants.Shared;
using System.Reflection;

namespace Plants.Domain.Infrastructure;

internal class EventSubscriber
{
    private readonly RepositoryCaller _caller;
    private readonly CqrsHelper _cqrs;
    private readonly IEventStore _eventStore;
    private readonly IServiceProvider _provider;

    public EventSubscriber(RepositoryCaller caller, CqrsHelper cqrs, IEventStore eventStore, IServiceProvider provider)
    {
        _caller = caller;
        _cqrs = cqrs;
        _eventStore = eventStore;
        _provider = provider;
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
        var applyerBaseType = typeof(TransposeApplyer<>);
        if (_cqrs.EventSubscriptions.TryGetValue(aggregate.Name, out var subscriptions))
        {
            foreach (var subscription in subscriptions)
            {
                var eventsToHandle = subscription.Filter.Match(
                  filter =>
                  {
                      var eventNames = filter.EventNames.Select(x => x.Replace("Event", ""));
                      return aggEvents.Where(x => eventNames.Contains(x.Metadata.Name));
                  },
                  all => aggEvents);
                if (eventsToHandle.Any())
                {
                    var receiverType = subscription.Transpose.GetType().GetGenericArguments()[1].GetGenericArguments()[0];
                    var applyerType = applyerBaseType.MakeGenericType(new[] { receiverType });
                    var applyer = _provider.GetRequiredService(applyerType);
                    var method = applyerType.GetMethod(nameof(TransposeApplyer<AggregateBase>.CallTransposeAsync), BindingFlags.Public | BindingFlags.Instance);
                    foreach (var @event in aggEvents)
                    {
                        var transposedEvent = (Event)await (dynamic)method.Invoke(applyer, new[] { subscription.Transpose, @event });
                        await _eventStore.AppendEventAsync(transposedEvent);
                        //TODO: Attach subscriber to event store instead of putting it here
                        await HandleEvents(new List<Event> { transposedEvent });
                    }
                }
            }
        }
    }

    private async Task HandleEvents(List<Event> events)
    {
        foreach (var (aggregate, aggEvents) in events.GroupBy(x => x.Metadata.Aggregate).Select(x => (x.Key, x.ToList())))
        {
            await this.UpdateAggregateAsync(aggregate, aggEvents);
            await this.UpdateSubscribersAsync(aggregate, aggEvents);
        }
    }
}
