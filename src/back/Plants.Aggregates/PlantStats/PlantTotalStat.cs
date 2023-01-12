using Plants.Aggregates.PlantInstructions;
using Plants.Aggregates.PlantOrders;
using Plants.Aggregates.PlantPosts;
using Plants.Aggregates.PlantStocks;

namespace Plants.Aggregates.PlantStats;

[Allow(Manager, Read)]
[Allow(Manager, Write)]
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
