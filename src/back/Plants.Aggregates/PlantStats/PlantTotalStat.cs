using Plants.Aggregates.PlantInstructions;
using Plants.Aggregates.PlantOrders;
using Plants.Aggregates.PlantPosts;
using Plants.Aggregates.PlantStocks;
using Plants.Domain.Aggregate;

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
              StatsHelper.CreateStatsSubscription<PlantTotalStat, PlantStock, StockAddedEvent>(StatsHelper.GetGroup, false);
    }

    private class PlantInstructionSubscription : IAggregateSubscription<PlantTotalStat, PlantInstruction>
    {
        public IEnumerable<EventSubscriptionBase<PlantTotalStat, PlantInstruction>> Subscriptions =>
            StatsHelper.CreateStatsSubscription<PlantTotalStat, PlantInstruction, InstructionCreatedEvent>(StatsHelper.GetGroup, false);
    }

    private class PlantPostSubscription : IAggregateSubscription<PlantTotalStat, PlantPost>
    {
        public IEnumerable<EventSubscriptionBase<PlantTotalStat, PlantPost>> Subscriptions =>
              StatsHelper.CreateStatsSubscription<PlantTotalStat, PlantPost, StockItemPostedEvent>(StatsHelper.GetGroup, false);
    }

    private class TimeStatsSubscriptions : IAggregateSubscription<PlantTotalStat, PlantOrder>
    {
        public IEnumerable<EventSubscriptionBase<PlantTotalStat, PlantOrder>> Subscriptions =>
            StatsHelper.CreateStatsSubscription<PlantTotalStat, PlantOrder, DeliveryConfirmedEvent>(StatsHelper.GetGroup, false);
    }
}
