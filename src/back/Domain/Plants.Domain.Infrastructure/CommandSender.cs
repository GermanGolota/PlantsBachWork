using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Persistence;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure;

internal class CommandSender : ICommandSender
{
    private readonly CqrsHelper _helper;
    private readonly ILogger<CommandSender> _logger;
    private readonly IEventStore _eventStore;
    private readonly RepositoryFactory _repositoryFactory;
    private readonly IServiceProvider _service;

    public CommandSender(CqrsHelper helper,
        ILogger<CommandSender> logger,
        IEventStore eventStore,
        RepositoryFactory repositoryFactory,
        IServiceProvider service)
    {
        _helper = helper;
        _logger = logger;
        _eventStore = eventStore;
        _repositoryFactory = repositoryFactory;
        _service = service;
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
                    var loadFunction = _repositoryFactory.CreateFor(command.Aggregate.Name);
                    var aggregate = await loadFunction(command.Aggregate.Id);
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
    }
}
