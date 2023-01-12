using Plants.Aggregates.PlantInstructions;
using Plants.Aggregates.PlantOrders;
using Plants.Aggregates.PlantPosts;
using Plants.Aggregates.PlantStocks;

namespace Plants.Aggregates.PlantStats;

[Allow(Manager, Read)]
[Allow(Manager, Write)]
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
