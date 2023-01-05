using Microsoft.Extensions.DependencyInjection;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Infrastructure.Domain.Helpers;
using System.Reflection;

namespace Plants.Domain.Infrastructure;

internal class EventSubscriber
{
    private readonly RepositoriesCaller _caller;
    private readonly CqrsHelper _cqrs;
    private readonly IEventStore _eventStore;
    private readonly IServiceProvider _provider;

    public EventSubscriber(RepositoriesCaller caller, CqrsHelper cqrs, IEventStore eventStore, IServiceProvider provider)
    {
        _caller = caller;
        _cqrs = cqrs;
        _eventStore = eventStore;
        _provider = provider;
    }

    public async Task ProcessCommandAsync(Command command, List<Event> aggEvents, CancellationToken token = default)
    {
        await UpdateProjectionAsync(command.Metadata.Aggregate, aggEvents, token);
        await UpdateSubscribersAsync(command, aggEvents, token);
    }

    private async Task UpdateProjectionAsync(AggregateDescription desc, IEnumerable<Event> newEvents, CancellationToken token = default)
    {
        var aggregate = await _caller.LoadAsync(desc, token);
        //would make sense for times in which we would load projection and apply events to it
        //var newAggregate = _eventApplyer.ApplyEventsTo(aggregate, newEvents);
        await _caller.InsertOrUpdateProjectionAsync(aggregate, token);
        await _caller.IndexProjectionAsync(aggregate, token);
    }

    private async Task UpdateSubscribersAsync(Command parentCommand, List<Event> aggEvents, CancellationToken token = default)
    {
        var aggregate = parentCommand.Metadata.Aggregate;
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
                    var applyerType = GetApplyerTypeFor(subscription);
                    var applyer = _provider.GetRequiredService(applyerType);
                    var method = applyerType.GetMethod(nameof(TransposeApplyer<AggregateBase>.CallTransposeAsync), BindingFlags.Public | BindingFlags.Instance);
                    var transposedEvents = (IEnumerable<Event>)await (dynamic)method.Invoke(applyer, new[] { subscription.Transpose, eventsToHandle, token })!;
                    var firstEvent = transposedEvents.FirstOrDefault();
                    if (firstEvent != default)
                    {
                        var commandNumber = firstEvent.Metadata.EventNumber - 1;
                        var command = parentCommand.ChangeTargetAggregate(firstEvent.Metadata.Aggregate);
                        commandNumber = await _eventStore.AppendCommandAsync(command, commandNumber, token);
                        await _eventStore.AppendEventsAsync(transposedEvents, commandNumber, command, token);
                    }
                }
            }
        }
    }

    private static Type GetApplyerTypeFor((OneOf<FilteredEvents, AllEvents> Filter, object Transpose) subscription)
    {
        var transposeType = subscription.Transpose.GetType();
        var receiverType = transposeType.GetGenericArguments()[0];
        Type applyerType;
        if (transposeType.IsAssignableToGenericType(typeof(AggregateLoadingTranspose<>)))
        {
            applyerType = typeof(TransposeApplyer<>).MakeGenericType(new[] { receiverType });
        }
        else
        {
            if (transposeType.IsAssignableToGenericType(typeof(AggregateLoadingTranspose<,>)))
            {
                var eventType = transposeType.GetGenericArguments()[1];
                applyerType = typeof(TransposeApplyer<,>).MakeGenericType(new[] { receiverType, eventType });
            }
            else
            {
                throw new Exception($"Unsupported transpose type - '{transposeType.FullName}'");
            }
        }

        return applyerType;
    }

}
