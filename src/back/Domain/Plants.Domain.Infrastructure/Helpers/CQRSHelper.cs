using Microsoft.Extensions.DependencyInjection;
using Plants.Domain;
using Plants.Shared;
using System.Reflection;

namespace Plants.Infrastructure.Domain.Helpers;

internal class CqrsHelper
{
    //addlist
    public IReadOnlyDictionary<Type, List<MethodInfo>> CommandHandlers { get; }
    public IReadOnlyDictionary<Type, List<MethodInfo>> EventHandlers { get; }
    //Aggregate to subscription
    public IReadOnlyDictionary<string, List<(Type SubscriberType, OneOf<FilteredEvents, AllEvents> Events)>> EventSubscribers { get; }

    public CqrsHelper(TypeHelper helper, IServiceProvider services)
    {
        var commandHandlerType = typeof(ICommandHandler<>);
        var domainHandler = typeof(IDomainCommandHandler<>);
        var eventHandler = typeof(IEventHandler<>);
        var eventSubscriber = typeof(IEventSubscriber);

        var commands = new Dictionary<Type, List<MethodInfo>>();
        var events = new Dictionary<Type, List<MethodInfo>>();
        var subs = new Dictionary<string, List<(Type SubscriberType, OneOf<FilteredEvents, AllEvents> Events)>>();
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

            if (type.IsAssignableTo(eventSubscriber))
            {
                var subscriber = (IEventSubscriber)services.GetRequiredService(type);
                subs.AddList(subscriber.Aggregate, (type, subscriber.Events));
            }
        }
        CommandHandlers = commands;
        EventHandlers = events;
        EventSubscribers = subs;
    }
}
