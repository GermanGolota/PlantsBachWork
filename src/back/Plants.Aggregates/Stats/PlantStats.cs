using Plants.Aggregates.PlantStocks;
using Plants.Shared;

namespace Plants.Aggregates.Stats;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
public class PlantStats : AggregateBase, IAggregateSubscription<PlantStats, PlantStock>, IEventHandler<StockAddedEvent>
{
    public PlantStats(Guid id) : base(id)
    {
    }

    public string GroupName { get; private set; } = null;
    public int PlantsCount { get; private set; } = 0;

    public void Handle(StockAddedEvent @event)
    {
        GroupName = @event.Plant.GroupName;
        PlantsCount++;
    }

    public IEnumerable<EventSubscriptionBase<PlantStats, PlantStock>> Subscriptions => new[]
    {
        new EventSubscription<PlantStats, PlantStock, StockAddedEvent>(
            new AllEvents(),
            new AggregateLoadingTranspose<PlantStats, StockAddedEvent>(
                @event => @event.Plant.GroupName.ToGuid(),
                (events, stats) =>
                    events.Select(@event => stats.TransposeSubscribedEvent(@event)))
            )
    };

}
