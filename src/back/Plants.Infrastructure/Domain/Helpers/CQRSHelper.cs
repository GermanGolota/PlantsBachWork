using Plants.Domain;
using Plants.Infrastructure.Helpers;
using System.Reflection;

namespace Plants.Infrastructure.Domain.Helpers;

public class CQRSHelper
{
    //addlist
    public IReadOnlyDictionary<Type, List<MethodInfo>> CommandHandlers { get; }
    public IReadOnlyDictionary<Type, List<MethodInfo>> EventHandlers { get; }

    public CQRSHelper(TypeHelper helper)
    {
        var commands = new Dictionary<Type, List<MethodInfo>>();
        var events = new Dictionary<Type, List<MethodInfo>>();
        foreach (var type in helper.Types)
        {
            if (type.IsAssignableToGenericType(typeof(IDomainCommandHandler<>)))
            {
                //todo: actually find type of cmd, instead of getting first
                var commandType = type.GetGenericArguments()[0];
                var method = type.GetMethod(nameof(IDomainCommandHandler<Command>.Handle));
                commands.AddList(commandType, method!);
            }
            else
            {
                if (type.IsAssignableToGenericType(typeof(ICommandHandler<>)))
                {
                    var commandType = type.GetGenericArguments()[0];
                    var method = type.GetMethod(nameof(ICommandHandler<Command>.HandleAsync));
                    commands.AddList(commandType, method!);
                }
                else
                {
                    if (type.IsAssignableToGenericType(typeof(IEventHandler<>)))
                    {
                        var eventType = type.GetGenericArguments()[0];
                        var method = type.GetMethod(nameof(IEventHandler<Event>.Handle));
                        events.AddList(eventType, method!);
                    }
                }
            }
        }
        CommandHandlers = commands;
        EventHandlers = events;
    }
}
