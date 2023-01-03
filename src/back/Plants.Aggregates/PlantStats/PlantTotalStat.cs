using Plants.Aggregates.PlantInstructions;
using Plants.Aggregates.PlantOrders;
using Plants.Aggregates.PlantPosts;
using Plants.Aggregates.PlantStocks;

namespace Plants.Aggregates.PlantStats;

public record GroupSelectedEvent(EventMetadata Metadata, string GroupName) : Event(Metadata);

public abstract class PlantStat : AggregateBase,
    IEventHandler<StockAddedEvent>, IEventHandler<InstructionCreatedEvent>,
    IEventHandler<StockItemPostedEvent>, IEventHandler<DeliveryConfirmedEvent>
{
    protected PlantStat(Guid id) : base(id)
    {
    }

    public long PlantsCount { get; private set; } = 0;
    public long InstructionsCount { get; private set; } = 0;
    public long PostedCount { get; private set; } = 0;
    public long SoldCount { get; private set; } = 0;
    public decimal Income { get; private set; } = 0;

    public void Handle(StockAddedEvent @event)
    {
        PlantsCount++;
    }

    public void Handle(InstructionCreatedEvent @event)
    {
        InstructionsCount++;
    }

    public void Handle(StockItemPostedEvent @event)
    {
        PostedCount++;
    }

    public void Handle(DeliveryConfirmedEvent @event)
    {
        SoldCount++;
        Income += @event.Price;
    }

}

//subscriptions work with no authorization
[Allow(Manager, Read)]
public class PlantTotalStat : PlantStat, IEventHandler<GroupSelectedEvent>
{
    public PlantTotalStat(Guid id) : base(id)
    {
    }

    public string GroupName { get; private set; } = null;

    public void Handle(GroupSelectedEvent @event)
    {
        GroupName = @event.GroupName;
    }

    private class PlantStockSubscription : IAggregateSubscription<PlantTotalStat, PlantStock>
    {
        public IEnumerable<EventSubscriptionBase<PlantTotalStat, PlantStock>> Subscriptions =>
              StatsHelper.CreateStatsSubscription<PlantTotalStat, PlantStock, StockAddedEvent>(StatsHelper.GetGroup, true);
    }

    private class PlantInstructionSubscription : IAggregateSubscription<PlantTotalStat, PlantInstruction>
    {
        public IEnumerable<EventSubscriptionBase<PlantTotalStat, PlantInstruction>> Subscriptions =>
            StatsHelper.CreateStatsSubscription<PlantTotalStat, PlantInstruction, InstructionCreatedEvent>(StatsHelper.GetGroup, true);
    }

    private class PlantPostSubscription : IAggregateSubscription<PlantTotalStat, PlantPost>
    {
        public IEnumerable<EventSubscriptionBase<PlantTotalStat, PlantPost>> Subscriptions =>
              StatsHelper.CreateStatsSubscription<PlantTotalStat, PlantPost, StockItemPostedEvent>(StatsHelper.GetGroup, true);
    }

    private class TimeStatsSubscriptions : IAggregateSubscription<PlantTotalStat, PlantOrder>
    {
        public IEnumerable<EventSubscriptionBase<PlantTotalStat, PlantOrder>> Subscriptions =>
            StatsHelper.CreateStatsSubscription<PlantTotalStat, PlantOrder, DeliveryConfirmedEvent>(StatsHelper.GetGroup, true);
    }
}

//subscriptions work with no authorization
[Allow(Manager, Read)]
public class PlantTimedStat : PlantStat, IEventHandler<GroupSelectedEvent>
{
    public PlantTimedStat(Guid id) : base(id)
    {
    }

    public string GroupName { get; private set; }
    public DateOnly Date { get; private set; }

    public void Handle(GroupSelectedEvent @event)
    {
        GroupName = @event.GroupName;
        Date = DateOnly.FromDateTime(@event.Metadata.Time);
    }

    private class PlantStockSubscription : IAggregateSubscription<PlantTimedStat, PlantStock>
    {
        public IEnumerable<EventSubscriptionBase<PlantTimedStat, PlantStock>> Subscriptions =>
              StatsHelper.CreateStatsSubscription<PlantTimedStat, PlantStock, StockAddedEvent>(StatsHelper.GetGroup, true);
    }

    private class PlantInstructionSubscription : IAggregateSubscription<PlantTimedStat, PlantInstruction>
    {
        public IEnumerable<EventSubscriptionBase<PlantTimedStat, PlantInstruction>> Subscriptions =>
            StatsHelper.CreateStatsSubscription<PlantTimedStat, PlantInstruction, InstructionCreatedEvent>(StatsHelper.GetGroup, true);
    }

    private class PlantPostSubscription : IAggregateSubscription<PlantTimedStat, PlantPost>
    {
        public IEnumerable<EventSubscriptionBase<PlantTimedStat, PlantPost>> Subscriptions =>
              StatsHelper.CreateStatsSubscription<PlantTimedStat, PlantPost, StockItemPostedEvent>(StatsHelper.GetGroup, true);
    }

    private class TimeStatsSubscriptions : IAggregateSubscription<PlantTimedStat, PlantOrder>
    {
        public IEnumerable<EventSubscriptionBase<PlantTimedStat, PlantOrder>> Subscriptions =>
            StatsHelper.CreateStatsSubscription<PlantTimedStat, PlantOrder, DeliveryConfirmedEvent>(StatsHelper.GetGroup, true);
    }
}

internal static class StatsHelper
{
    public static IEnumerable<EventSubscriptionBase<TStats, TSub>> CreateStatsSubscription<TStats, TSub, TEvent>(
        Func<TEvent, string> getGroupName, bool isTimed) where TStats : PlantStat where TSub : AggregateBase where TEvent : Event
        => new[]
       {
            EventSubscriptionFactory.CreateForwarded<TStats, TSub, TEvent>(
                @event => isTimed ? @event.TimedId(getGroupName(@event)) : getGroupName(@event).ToGuid(),
                    (stat, events) =>
                    stat.CommandsProcessed < 1
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