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
    public IReadOnlyDictionary<string, List<(OneOf<FilteredEvents, AllEvents> Filter, object Transpose)>> EventSubscriptions { get; }

    public CqrsHelper(TypeHelper helper)
    {
        var commandHandlerType = typeof(ICommandHandler<>);
        var domainHandler = typeof(IDomainCommandHandler<>);
        var eventHandler = typeof(IEventHandler<>);
        var subscriptionType = typeof(IAggregateSubscription<,>);
        var aggregateType = typeof(AggregateBase);
        var eventSubscriptionType = typeof(EventSubscription<,>);

        var commands = new Dictionary<Type, List<MethodInfo>>();
        var events = new Dictionary<Type, List<MethodInfo>>();
        var subs = new Dictionary<string, List<(OneOf<FilteredEvents, AllEvents> filter, object Transpose)>>();
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

            if (type.IsStrictlyAssignableToGenericType(subscriptionType))
            {
                var subscriptionInterface = type.GetImplementations(subscriptionType).Single();
                if (subscriptionInterface.GetGenericArguments() is [Type receiver, Type transmitter])
                {
                    var subscriptionsProp = type.GetProperty(nameof(IAggregateSubscription<AggregateBase, AggregateBase>.Subscriptions), BindingFlags.Static | BindingFlags.Public);
                    var subscriptions = (IEnumerable<object>)subscriptionsProp.GetValue(null);
                    foreach (var subscription in subscriptions)
                    {
                        var currentSubscriptionType = eventSubscriptionType.MakeGenericType(receiver, transmitter);
                        var filterProp = currentSubscriptionType.GetProperty(nameof(EventSubscription<AggregateBase, AggregateBase>.EventFilter));
                        var transposeProp = currentSubscriptionType.GetProperty(nameof(EventSubscription<AggregateBase, AggregateBase>.TransposeEvent));
                        var filter = (OneOf<FilteredEvents, AllEvents>)filterProp.GetValue(subscription);
                        var transpose = transposeProp.GetValue(subscription);

                        subs.AddList(transmitter.Name, (filter, transpose));
                    }
                }
                else
                {
                    throw new Exception("Cannot find type definitions for subscriptions");
                }
            }
        }

        CommandHandlers = commands;
        EventHandlers = events;
        EventSubscriptions = subs;
    }
}
