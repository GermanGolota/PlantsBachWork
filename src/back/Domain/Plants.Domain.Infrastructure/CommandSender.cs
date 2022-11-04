using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plants.Domain.Persistence;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure;

internal class CommandSender : ICommandSender
{
    private readonly CQRSHelper _helper;
    private readonly ILogger<CommandSender> _logger;
    private readonly IServiceProvider _service;
    private readonly AggregateHelper _aggregate;
    private readonly IEventStore _eventStore;

    public CommandSender(CQRSHelper helper, 
        ILogger<CommandSender> logger, 
        IServiceProvider service, 
        AggregateHelper aggregate,
        IEventStore eventStore)
    {
        _helper = helper;
        _logger = logger;
        _service = service;
        _aggregate = aggregate;
        _eventStore = eventStore;
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

        if (_helper.CommandHandlers.TryGetValue(commandType, out var handlers))
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
                    var aggregateId = command.Aggregate.Id;
                    var aggregateName = command.Aggregate.Name;
                    if (_aggregate.Aggregates.TryGetValue(aggregateName, out var aggregateType))
                    {
                        var repositoryType = typeof(IRepository<>).MakeGenericType(aggregateType);
                        var repository = _service.GetRequiredService(repositoryType);
                        var method = repository.GetType().GetMethod(nameof(IRepository<AggregateBase>.GetByIdAsync));
                        AggregateBase aggregate = (AggregateBase)await (dynamic)method.Invoke(repository, new object[] { aggregateId });
                        var newEvents = (IEnumerable<Event>)handler.Invoke(aggregate, new object[] { command });
                        events.AddRange(newEvents);
                    }
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
    }
}
