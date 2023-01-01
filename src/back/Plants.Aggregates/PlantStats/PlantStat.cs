using Plants.Aggregates.PlantStocks;

namespace Plants.Aggregates.PlantStats;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
public class PlantStat : AggregateBase, IEventHandler<StockAddedEvent>
{
    public PlantStat(Guid id) : base(id)
    {
    }

    public string GroupName { get; private set; } = null;
    public int PlantsCount { get; private set; } = 0;

    public void Handle(StockAddedEvent @event)
    {
        GroupName = @event.Plant.GroupName;
        PlantsCount++;
    }

    private class PlantStatsStockSubscription : IAggregateSubscription<PlantStat, PlantStock>
    {
        public IEnumerable<EventSubscriptionBase<PlantStat, PlantStock>> Subscriptions => new[]
        {
            new EventSubscription<PlantStat, PlantStock, StockAddedEvent>(
                new AllEvents(),
                new AggregateLoadingTranspose<PlantStat, StockAddedEvent>(
                    @event => @event.Plant.GroupName.ToGuid(),
                    (events, stats) =>
                        events.Select(@event => stats.TransposeSubscribedEvent(@event)))
                )
        };

    }

}
