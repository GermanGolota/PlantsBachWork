namespace Plants.Aggregates;

internal static class StatsHelper
{
    public static IEnumerable<EventSubscriptionBase<TStats, TSub>> CreateStatsSubscription<TStats, TSub, TEvent>(
        Func<TEvent, string> getGroupName, bool isTimed) where TStats : PlantStat where TSub : AggregateBase where TEvent : Event
        => new[]
       {
            EventSubscriptionFactory.CreateForwarded<TStats, TSub, TEvent>(
                @event => isTimed ? @event.TimedId(getGroupName(@event)) : getGroupName(@event).ToGuid(),
                    (stat, events) =>
                    stat.Metadata.CommandsProcessed < 1
                        ? new[] {new GroupSelectedEvent(EventFactory.Shared.CreateForSubscription<GroupSelectedEvent>(stat, events.First()), getGroupName(events.First()))}
                        : Array.Empty<Event>()
                    )
        };

    private static Guid TimedId(this Event @event, string str) =>
        $"{str}{DateOnly.FromDateTime(@event.Metadata.Time)}".ToGuid();

    internal static string GetGroup(this DeliveryConfirmedEvent @event) =>
        @event.GroupName;

    internal static string GetGroup(this StockItemPostedEvent @event) =>
        @event.GroupName;

    internal static string GetGroup(this InstructionCreatedEvent @event) =>
        @event.Instruction.GroupName;

    internal static string GetGroup(this StockAddedEvent @event) =>
       @event.Plant.GroupName;

}