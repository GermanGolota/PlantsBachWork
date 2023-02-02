using System.Reflection;

namespace Plants.Domain.Infrastructure;

internal class CqrsHelper
{
    public IReadOnlyDictionary<Type, List<(MethodInfo Checker, MethodInfo Handler)>> CommandHandlers { get; }
    public IReadOnlyDictionary<Type, List<MethodInfo>> EventHandlers { get; }
    //Aggregate to subscription
    public IReadOnlyDictionary<string, List<(OneOf<FilteredEvents, AllEvents> Filter, object Transpose)>> EventSubscriptions { get; }

    public CqrsHelper(TypeHelper helper, AggregateHelper aggregateHelper)
    {
        var identityType = typeof(IUserIdentity);
        var commandHandlerType = typeof(ICommandHandler<>);
        var domainHandler = typeof(IDomainCommandHandler<>);
        var eventHandler = typeof(IEventHandler<>);
        var subscriptionType = typeof(IAggregateSubscription<,>);
        var aggregateType = typeof(AggregateBase);
        var eventSubscriptionType = typeof(EventSubscription<,>);

        var commands = new Dictionary<Type, List<(MethodInfo Checker, MethodInfo Handler)>>();
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
                    var handleMethod = type.GetMethod(nameof(IDomainCommandHandler<Command>.Handle), new[] { commandType })!;
                    var checkMethod = type.GetMethod(nameof(IDomainCommandHandler<Command>.ShouldForbid), new[] { commandType, identityType })!;
                    if (handleMethod.DeclaringType == type)
                    {
                        commands.AddList(commandType, (checkMethod, handleMethod));
                    }
                }
            }

            if (type.IsStrictlyAssignableToGenericType(commandHandlerType))
            {
                var handlers = type.GetImplementations(commandHandlerType);
                foreach (var handler in handlers)
                {
                    var commandType = handler.GetGenericArguments()[0];
                    var handleMethod = type.GetMethod(nameof(ICommandHandler<Command>.HandleAsync), new[] { commandType, typeof(CancellationToken) })!;
                    var checkMethod = type.GetMethod(nameof(ICommandHandler<Command>.ShouldForbidAsync), new[] { commandType, identityType, typeof(CancellationToken) })!;
                    if (handleMethod.DeclaringType == type)
                    {
                        commands.AddList(commandType, (checkMethod, handleMethod));
                    }
                }
            }

            if (type.IsStrictlyAssignableToGenericType(eventHandler))
            {
                var handlers = type.GetImplementations(eventHandler);
                foreach (var handler in handlers)
                {
                    var eventType = handler.GetGenericArguments()[0];
                    var method = type.GetMethod(nameof(IEventHandler<Event>.Handle), new[] { eventType })!;
                    if (method.DeclaringType == type)
                    {
                        events.AddList(eventType, method);
                    }
                }
            }

            if (type.IsStrictlyAssignableToGenericType(subscriptionType))
            {
                var subscriptionInterface = type.GetImplementations(subscriptionType).Single();
                if (subscriptionInterface.GetGenericArguments() is [Type receiver, Type transmitter])
                {
                    var subscriptionsProp = type.GetProperty(nameof(IAggregateSubscription<AggregateBase, AggregateBase>.Subscriptions))!;
                    var value = type.IsAssignableTo(aggregateType)
                        ? aggregateHelper.AggregateCtors[receiver].Invoke(new object[] { Guid.Empty })
                        : Activator.CreateInstance(type);
                    var subscriptions = (IEnumerable<object>)subscriptionsProp.GetValue(value)!;
                    foreach (var subscription in subscriptions)
                    {
                        var subType = subscription.GetType();
                        var filter = (OneOf<FilteredEvents, AllEvents>)subType
                            .GetProperty(nameof(EventSubscription<AggregateBase, AggregateBase>.EventFilter))
                            !.GetValue(subscription)!;
                        var transpose = subType
                            .GetProperty(nameof(EventSubscription<AggregateBase, AggregateBase>.TransposeEvent))
                            !.GetValue(subscription)!;

                        subs.AddList(transmitter.Name, (filter, transpose));
                    }
                }
                else
                {
                    throw new Exception("Cannot find type definitions for subscriptions (subscriptions have to be placed on aggregates)");
                }
            }
        }

        CommandHandlers = commands;
        EventHandlers = events;
        EventSubscriptions = subs;
    }
}
