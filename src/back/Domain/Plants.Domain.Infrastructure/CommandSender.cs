using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Persistence;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure;

internal class CommandSender : ICommandSender
{
    private readonly CqrsHelper _cqrs;
    private readonly ILogger<CommandSender> _logger;
    private readonly IEventStore _eventStore;
    private readonly RepositoryCaller _caller;
    private readonly IServiceProvider _service;
    private readonly EventSubscriber _subscriber;

    public CommandSender(CqrsHelper cqrs,
        ILogger<CommandSender> logger,
        IEventStore eventStore,
        RepositoryCaller caller,
        IServiceProvider service,
        EventSubscriber subscriber)
    {
        _cqrs = cqrs;
        _logger = logger;
        _eventStore = eventStore;
        _caller = caller;
        _service = service;
        _subscriber = subscriber;
    }

    public async Task SendCommandAsync(Command command)
    {
        var events = new List<Event>();
        var commandType = command.GetType();
        if (commandType == typeof(Command))
        {
            _logger.LogError("Tried to send command with no type");
            throw new Exception("Can't send generic command!");
        }

        if (_cqrs.CommandHandlers.TryGetValue(commandType, out var handlers))
        {
            foreach (var handler in handlers)
            {
                //external cmd
                if (handler.ReturnType.IsAssignableTo(typeof(Task<IEnumerable<Event>>)))
                {
                    var service = _service.GetRequiredService(handler.DeclaringType!);
                    var task = (Task<IEnumerable<Event>>)handler.Invoke(service, new object[] { command })!;
                    events.AddRange(await task);
                }
                else
                {
                    //aggregate command
                    var aggregate = await _caller.LoadAsync(command.Metadata.Aggregate);
                    var newEvents = (IEnumerable<Event>)handler.Invoke(aggregate, new object[] { command });
                    events.AddRange(newEvents);
                }
            }
        }
        else
        {
            _logger.LogError("Send command with no handlers");
        }

        foreach (var @event in events)
        {
            await _eventStore.AppendEventAsync(@event);
        }

        //TODO: Attach subscriber to event store instead of putting it here
        await HandleEvents(events);
    }

    private async Task HandleEvents(List<Event> events)
    {
        foreach (var (aggregate, aggEvents) in events.GroupBy(x => x.Metadata.Aggregate).Select(x => (x.Key, x.ToList())))
        {
            //primary aggregate
            await _subscriber.UpdateAggregateAsync(aggregate, aggEvents);
            await _subscriber.UpdateSubscribersAsync(aggregate, aggEvents);
        }
    }
}
