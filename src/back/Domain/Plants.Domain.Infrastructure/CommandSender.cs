using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Persistence;
using Plants.Domain.Services;
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
    private readonly IIdentityProvider _identityProvider;
    private readonly AccessesHelper _accesses;

    public CommandSender(CqrsHelper cqrs,
        ILogger<CommandSender> logger,
        IEventStore eventStore,
        RepositoryCaller caller,
        IServiceProvider service,
        EventSubscriber subscriber,
        IIdentityProvider identityProvider,
        AccessesHelper accesses)
    {
        _cqrs = cqrs;
        _logger = logger;
        _eventStore = eventStore;
        _caller = caller;
        _service = service;
        _subscriber = subscriber;
        _identityProvider = identityProvider;
        _accesses = accesses;
    }

    public async Task<OneOf<CommandAcceptedResult, CommandForbidden>> SendCommandAsync(Command command)
    {
        var commandType = command.GetType();
        if (commandType == typeof(Command))
        {
            _logger.LogError("Tried to send command with no type");
            throw new Exception("Can't send generic command!");
        }
        var identity = _identityProvider.Identity!;
        var commandAggregate = command.Metadata.Aggregate;
        OneOf<CommandAcceptedResult, CommandForbidden> result;
        if (UserHasAccess(identity, commandAggregate))
        {
            if (_cqrs.CommandHandlers.TryGetValue(commandType, out var handlePairs))
            {
                result = await ExecuteCommand(command, commandAggregate, handlePairs);
            }
            else
            {
                _logger.LogError("Send command with no handlers");
                result = new CommandForbidden("Failed to find any handler for command");
            }
        }
        else
        {
            result = new CommandForbidden($"Cannot perform any updates against '{commandAggregate.Name}'");
        }

        return result;
    }

    private bool UserHasAccess(IUserIdentity identity, AggregateDescription commandAggregate) =>
        identity.Roles.Contains(UserRole.Manager)
        || _accesses.AggregateToWriteRoles[commandAggregate.Name].Intersect(identity.Roles).Any();

    private async Task<OneOf<CommandAcceptedResult, CommandForbidden>> ExecuteCommand(Command command, AggregateDescription commandAggregate, List<(MethodInfo Checker, MethodInfo Handler)> handlePairs)
    {
        var aggregate = await _caller.LoadAsync(commandAggregate);
        var commandVersion = aggregate.Version;
        await _eventStore.AppendCommandAsync(command, commandVersion);
        //TODO: Move the rest to sub?
        var checkResults = await PerformChecks(command, handlePairs);
        var checkFailures = checkResults.Where(_ => _.CheckFailure.HasValue).Select(_ => _.CheckFailure!.Value);
        OneOf<CommandAcceptedResult, CommandForbidden> result;
        if (checkFailures.Any())
        {
            var reasons = checkFailures.Select(failure => failure.Reasons).Flatten().ToArray();
            result = await CreateFailure(command, commandVersion, reasons, false);
        }
        else
        {
            try
            {
                var events = await ExecuteHandlers(command, checkResults);
                foreach (var @event in events)
                {
                    await _eventStore.AppendEventAsync(@event);
                }
                //TODO: Attach subscriber to event store instead of putting it here
                await HandleEvents(events, command);

                result = new CommandAcceptedResult();
            }
            catch (Exception e)
            {
                result = await CreateFailure(command, commandVersion, new[] { e.Message, e.ToString() }, true);
            }
        }

        return result;
    }

    private async Task<OneOf<CommandAcceptedResult, CommandForbidden>> CreateFailure(Command command, ulong commandVersion, string[] reasons, bool isException)
    {
        var metadata = EventFactory.Shared.Create<FailEvent>(command, commandVersion + 1);
        await _eventStore.AppendEventAsync(new FailEvent(metadata, reasons, isException));
        return new CommandForbidden(reasons);
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

    private async Task<List<(CommandForbidden? CheckFailure, MethodInfo Handle, OneOf<AggregateBase, object> Dependency)>> PerformChecks(Command command, List<(MethodInfo Checker, MethodInfo Handler)> handlePairs)
    {
        var identity = _identityProvider.Identity!;

        List<(CommandForbidden? CheckResult, MethodInfo Handle, OneOf<AggregateBase, object>)> checkResults = new();
        foreach (var (check, handle) in handlePairs)
        {
            //external cmd
            if (check.ReturnType.IsAssignableTo(typeof(Task)))
            {
                var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
                var service = _service.GetRequiredService(handlerType);
                var task = (Task<CommandForbidden?>)check.Invoke(service, new object[] { command, identity })!;
                var dependency = new OneOf<AggregateBase, object>(service);
                checkResults.Add((await task, handle, dependency));
            }
            else
            {
                //aggregate command
                var aggregate = await _caller.LoadAsync(command.Metadata.Aggregate);
                var checkResult = (CommandForbidden?)check.Invoke(aggregate, new object[] { command, identity });
                var dependency = new OneOf<AggregateBase, object>(aggregate);
                checkResults.Add((checkResult, handle, dependency));
            }
        }

        return checkResults;
    }

    private async Task HandleEvents(List<Event> events, Command command)
    {
        foreach (var (aggregate, aggEvents) in events.GroupBy(x => x.Metadata.Aggregate).Select(x => (x.Key, x.ToList())))
        {
            await _subscriber.UpdateAggregateAsync(aggregate, aggEvents);
            await _subscriber.UpdateSubscribersAsync(aggregate, aggEvents, command);
        }
    }
}
