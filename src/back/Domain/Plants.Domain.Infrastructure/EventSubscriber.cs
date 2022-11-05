using Microsoft.Extensions.Logging;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure;

internal class EventSubscriber
{
    private readonly CqrsHelper _cqrs;
    private readonly ILogger<EventSubscriber> _logger;
    private readonly RepositoryCaller _caller;

    public EventSubscriber(
        CqrsHelper cqrs,
        ILogger<EventSubscriber> logger,
        RepositoryCaller caller)
    {
        _cqrs = cqrs;
        _logger = logger;
        _caller = caller;
    }

    //TODO: Think about processing multiple events at a time
    public async Task ProcessEvent(Event @event)
    {
        var eventType = @event.GetType();
        if (_cqrs.EventHandlers.TryGetValue(eventType, out var handlers))
        {
            var aggregate = await _caller.LoadAsync(@event.Aggregate);
            foreach (var handler in handlers)
            {
                handler.Invoke(aggregate, new object[] { @event });
                await _caller.UpdateAsync(aggregate);
            }
        }
        else
        {
            _logger.LogWarning("No event subscriber for '{type}'", eventType);
        }
    }
}
