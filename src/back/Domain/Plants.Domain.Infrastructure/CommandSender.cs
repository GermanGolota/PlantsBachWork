using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Persistence;
using Plants.Infrastructure.Domain.Helpers;
using Plants.Shared;
using System.Reflection;

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

    public async Task<OneOf<CommandAcceptedResult, CommandForbidden>> SendCommandAsync(Command command)
    {
        var commandType = command.GetType();
        if (commandType == typeof(Command))
        {
            _logger.LogError("Tried to send command with no type");
            throw new Exception("Can't send generic command!");
        }
        OneOf<CommandAcceptedResult, CommandForbidden> result;
        if (_cqrs.CommandHandlers.TryGetValue(commandType, out var handlePairs))
        {
            var checkResults = await PerformChecks(command, handlePairs);
            var checkFailures = checkResults.Where(_ => _.CheckFailure.HasValue).Select(_ => _.CheckFailure!.Value);
            if (checkFailures.Any())
            {
                var reasons = checkFailures.Select(failure => failure.Reasons).Flatten().ToArray();
                result = new CommandForbidden(reasons);
            }
            else
            {
                var events = await ExecuteHandlers(command, checkResults);
                foreach (var @event in events)
                {
                    await _eventStore.AppendEventAsync(@event);
                }

                //TODO: Attach subscriber to event store instead of putting it here
                await HandleEvents(events);

                result = new CommandAcceptedResult();
            }
        }
        else
        {
            _logger.LogError("Send command with no handlers");
            result = new CommandForbidden("Failed to find any handler for command");
        }

        return result;
    }

    private static async Task<List<Event>> ExecuteHandlers(Command command, List<(CommandForbidden? CheckFailure, MethodInfo Handle, OneOf<AggregateBase, object>)> checkResults)
    {
        List<Event> events = new();
        foreach (var (_, handle, dependency) in checkResults)
        {
            var newEvents = await dependency.MatchAsync(
                aggregate =>
                {
                    return Task.FromResult((IEnumerable<Event>)handle.Invoke(aggregate, new object[] { command }));
                },
                async service =>
                {
                    return await (Task<IEnumerable<Event>>)handle.Invoke(service, new object[] { command })!;
                });
            events.AddRange(newEvents);
        }

        return events;
    }

    private async Task<List<(CommandForbidden? CheckFailure, MethodInfo Handle, OneOf<AggregateBase, object>)>> PerformChecks(Command command, List<(MethodInfo Checker, MethodInfo Handler)> handlePairs)
    {
        List<(CommandForbidden? CheckResult, MethodInfo Handle, OneOf<AggregateBase, object>)> checkResults = new();
        foreach (var (check, handle) in handlePairs)
        {
            //external cmd
            if (check.ReturnType.IsAssignableTo(typeof(Task)))
            {
                var service = _service.GetRequiredService(check.DeclaringType!);
                var task = (Task<CommandForbidden?>)check.Invoke(service, new object[] { command })!;
                var dependency = new OneOf<AggregateBase, object>(service);
                checkResults.Add((await task, handle, dependency));
            }
            else
            {
                //aggregate command
                var aggregate = await _caller.LoadAsync(command.Metadata.Aggregate);
                var checkResult = (CommandForbidden?)check.Invoke(aggregate, new object[] { command });
                var dependency = new OneOf<AggregateBase, object>(aggregate);
                checkResults.Add((checkResult, handle, dependency));
            }
        }

        return checkResults;
    }

    private async Task HandleEvents(List<Event> events)
    {
        foreach (var (aggregate, aggEvents) in events.GroupBy(x => x.Metadata.Aggregate).Select(x => (x.Key, x.ToList())))
        {
            await _subscriber.UpdateAggregateAsync(aggregate, aggEvents);
            await _subscriber.UpdateSubscribersAsync(aggregate, aggEvents);
        }
    }
}
