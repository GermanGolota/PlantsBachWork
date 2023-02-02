using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Plants.Domain.Infrastructure;

internal class CommandSender : ICommandSender
{
    private readonly CqrsHelper _cqrs;
    private readonly ILogger<CommandSender> _logger;
    private readonly IEventStore _eventStore;
    private readonly RepositoriesCaller _caller;
    private readonly IServiceProvider _service;
    private readonly IIdentityProvider _identityProvider;
    private readonly AccessesHelper _accesses;
    private readonly ISubscriptionProcessingNotificator _notificator;
    private readonly ISubscriptionProcessingMarker _marker;

    public CommandSender(CqrsHelper cqrs,
        ILogger<CommandSender> logger,
        IEventStore eventStore,
        RepositoriesCaller caller,
        IServiceProvider service,
        IIdentityProvider identityProvider,
        AccessesHelper accesses,
        ISubscriptionProcessingNotificator notificator,
        ISubscriptionProcessingMarker marker)
    {
        _cqrs = cqrs;
        _logger = logger;
        _eventStore = eventStore;
        _caller = caller;
        _service = service;
        _identityProvider = identityProvider;
        _accesses = accesses;
        _notificator = notificator;
        _marker = marker;
    }

    public async Task<OneOf<CommandAcceptedResult, CommandForbidden>> SendCommandAsync(Command command, CommandExecutionOptions options, CancellationToken token = default)
    {
        _logger.LogInformation("Sending command '{commandId}' into '{@aggregate}'", command.Metadata.Id, command.Metadata.Aggregate);
        var commandType = command.GetType();
        if (commandType == typeof(Command))
        {
            _logger.LogError("Tried to send command with no type");
            throw new Exception("Can't send generic command!");
        }
        var commandAggregate = command.Metadata.Aggregate;
        OneOf<CommandAcceptedResult, CommandForbidden> result;
        if (UserHasAccess(_identityProvider.Identity!, commandAggregate))
        {
            if (_cqrs.CommandHandlers.TryGetValue(commandType, out var handlePairs))
            {
                if (options is CommandExecutionOptions.Wait)
                {
                    _notificator.SubscribeToNotifications(commandAggregate);
                    _marker.MarkSubscribersCount(commandAggregate, 1);
                }
                result = await ExecuteCommand(command, commandAggregate, handlePairs, token);
            }
            else
            {
                _logger.LogError("Send command with no handlers");
                result = new CommandForbidden("Failed to find any handler for command");
            }
        }
        else
        {
            _logger.LogInformation("Unauthorized command tried to perform by '{username}'", command.Metadata.UserName);
            result = new CommandForbidden($"Cannot perform any updates against '{commandAggregate.Name}'");
        }

        if (options is CommandExecutionOptions.Wait wait)
        {
            var success = await WaitForSubscriptionAsync(commandAggregate, wait.TimeToWait, token);
            if (success is false)
            {
                _logger.LogInformation("Failed to wait to subscription to be processed for '{@aggregate}'", commandAggregate);
                throw new TimeoutException($"Timeout while waiting for subscription to be processed for '{commandAggregate.Id}' in '{commandAggregate.Name}'");
            }
            _notificator.UnsubscribeFromNotifications(commandAggregate);
        }

        _logger.LogInformation("Processed command '{commandId}' for '{@aggregate}'", command.Metadata.Id, command.Metadata.Aggregate);

        return result;
    }

    private async Task<bool> WaitForSubscriptionAsync(AggregateDescription aggregate, TimeSpan timeout, CancellationToken token) =>
        await Task.Run(async () =>
        {
            var watch = Stopwatch.StartNew();
            var delay = timeout.TotalSeconds < 10 ? timeout / 10 : TimeSpan.FromMilliseconds(250);

            _logger.LogInformation("Started waiting for subscription for '{@aggregate}'", aggregate);
            while (token.IsCancellationRequested is false)
            {
                _logger.LogDebug("Waiting for subscriptions for '{time}'", watch.Elapsed);

                if (_notificator.WasProcessed(aggregate))
                {
                    break;
                }

                await Task.Delay(delay, token);
            }

            watch.Stop();
            _logger.LogInformation("Finished waiting for subscription for '{@aggregate}'", aggregate);
        }, token)
        .ExecuteWithTimeoutAsync(timeout, token);

    private bool UserHasAccess(IUserIdentity identity, AggregateDescription commandAggregate) =>
        identity.Roles.Contains(UserRole.Manager)
        || _accesses.AggregateToWriteRoles[commandAggregate.Name].Intersect(identity.Roles).Any();

    private async Task<OneOf<CommandAcceptedResult, CommandForbidden>> ExecuteCommand(Command command, AggregateDescription commandAggregate, List<(MethodInfo Checker, MethodInfo Handler)> handlePairs, CancellationToken token = default)
    {
        var aggregate = await _caller.LoadAsync(commandAggregate, token: token);
        var commandVersion = await _eventStore.AppendCommandAsync(command, aggregate.Metadata.Version, token);

        var checkResults = await PerformChecksAsync(command, handlePairs, token);
        var checkFailures = checkResults.Where(_ => _.CheckFailure.HasValue).Select(_ => _.CheckFailure!.Value);
        OneOf<CommandAcceptedResult, CommandForbidden> result;
        if (checkFailures.Any())
        {
            var reasons = checkFailures.Select(failure => failure.Reasons).Flatten().ToArray();
            result = await AppendFailureAsync(command, commandVersion, reasons, false, token);
        }
        else
        {
            List<Event>? events = null;
            try
            {
                events = await ExecuteHandlersAsync(command, checkResults, token);
                await _eventStore.AppendEventsAsync(events, commandVersion, command, token);
                _logger.LogInformation("Successfully processes command '{commandId}' for '{aggregate}'", command.Metadata.Id, command.Metadata.Aggregate);
                result = new CommandAcceptedResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process events for command");
                var reasons = new[] { e.Message, e.ToString() };
                result = await AppendFailureAsync(command, commandVersion, reasons, true, token);
            }

        }

        return result;
    }

    private async Task<OneOf<CommandAcceptedResult, CommandForbidden>> AppendFailureAsync(Command command, ulong commandVersion, string[] reasons, bool isException, CancellationToken token = default)
    {
        await _eventStore.AppendEventsAsync(new[] { new FailEvent(EventFactory.Shared.Create<FailEvent>(command), reasons, isException) }, commandVersion, command, token);
        return new CommandForbidden(reasons);
    }

    private static async Task<List<Event>> ExecuteHandlersAsync(Command command, List<(CommandForbidden? CheckFailure, MethodInfo Handle, OneOf<AggregateBase, object>)> checkResults, CancellationToken token = default)
    {
        List<Event> events = new();
        foreach (var (_, handle, dependency) in checkResults)
        {
            var newEvents = await dependency.MatchAsync(
                aggregate =>
                {
                    return Task.FromResult((IEnumerable<Event>)handle.Invoke(aggregate, new object[] { command })!);
                },
                async service =>
                {
                    return await (Task<IEnumerable<Event>>)handle.Invoke(service, new object[] { command, token })!;
                });
            events.AddRange(newEvents);
        }

        return events;
    }

    private async Task<List<(CommandForbidden? CheckFailure, MethodInfo Handle, OneOf<AggregateBase, object> Dependency)>> PerformChecksAsync(Command command, List<(MethodInfo Checker, MethodInfo Handler)> handlePairs, CancellationToken token = default)
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
                var task = (Task<CommandForbidden?>)check.Invoke(service, new object[] { command, identity, token })!;
                var dependency = new OneOf<AggregateBase, object>(service);
                checkResults.Add((await task, handle, dependency));
            }
            else
            {
                //aggregate command
                var aggregate = await _caller.LoadAsync(command.Metadata.Aggregate, token: token);
                var checkResult = (CommandForbidden?)check.Invoke(aggregate, new object[] { command, identity });
                var dependency = new OneOf<AggregateBase, object>(aggregate);
                checkResults.Add((checkResult, handle, dependency));
            }
        }

        return checkResults;
    }

}
