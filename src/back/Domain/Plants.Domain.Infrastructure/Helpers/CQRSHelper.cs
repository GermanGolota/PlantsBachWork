using Plants.Domain;
using Plants.Shared;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Plants.Infrastructure.Domain.Helpers;

internal class CqrsHelper
{
    //addlist
    public IReadOnlyDictionary<Type, List<MethodInfo>> CommandHandlers { get; }
    public IReadOnlyDictionary<Type, List<MethodInfo>> EventHandlers { get; }

    public CqrsHelper(TypeHelper helper)
    {
        var commandHandlerType = typeof(ICommandHandler<>);
        var domainHandler = typeof(IDomainCommandHandler<>);
        var eventHandler = typeof(IEventHandler<>);

        var commands = new Dictionary<Type, List<MethodInfo>>();
        var events = new Dictionary<Type, List<MethodInfo>>();
        foreach (var type in helper.Types)
        {
            if (type.IsStrictlyAssignableToGenericType(domainHandler))
            {
                var handlers = type.GetImplementations(domainHandler);
                foreach (var handler in handlers)
                {
                    var commandType = handler.GetGenericArguments()[0];
                    var method = type.GetMethod(nameof(IDomainCommandHandler<Command>.Handle), new[] { commandType });
                    commands.AddList(commandType, method!);
                }
            }

            if (type.IsStrictlyAssignableToGenericType(commandHandlerType))
            {
                var handlers = type.GetImplementations(commandHandlerType);
                foreach (var handler in handlers)
                {
                    var commandType = handler.GetGenericArguments()[0];
                    var method = type.GetMethod(nameof(ICommandHandler<Command>.HandleAsync), new[] { commandType });
                    commands.AddList(commandType, method!);
                }
            }

            if (type.IsStrictlyAssignableToGenericType(eventHandler))
            {
                var handlers = type.GetImplementations(eventHandler);
                foreach (var handler in handlers)
                {
                    var eventType = handler.GetGenericArguments()[0];
                    var method = type.GetMethod(nameof(IEventHandler<Event>.Handle), new[] { eventType });
                    events.AddList(eventType, method!);
                }
            }
        }
        CommandHandlers = commands;
        EventHandlers = events;
    }
}
