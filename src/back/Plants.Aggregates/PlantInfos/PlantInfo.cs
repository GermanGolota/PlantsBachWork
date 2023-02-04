namespace Plants.Aggregates;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Manager, Read)]
[Allow(Manager, Write)]
public class PlantInfo : AggregateBase, IEventHandler<StockAddedEvent>
{
    //Id that is being used by plant info singleton
    public static Guid InfoId { get; } = Guid.Parse("1eebef8d-ba56-406f-a9f5-bc21c1a9ca96");
    public PlantInfo(Guid id) : base(InfoId)
    {
        if (id != Guid.Empty && id != InfoId)
        {
            throw new InvalidOperationException("Cannot use non-main plant info aggregate");
        }
    }

    public HashSet<string> GroupNames { get; private set; } = new();
    public HashSet<string> RegionNames { get; private set; } = new();
    public HashSet<string> SoilNames { get; private set; } = new();

    public void Handle(StockAddedEvent @event)
    {
        var plant = @event.Plant;

        GroupNames.Add(plant.GroupName);
        SoilNames.Add(plant.SoilName);
        foreach (var regionName in plant.RegionNames)
        {
            RegionNames.Add(regionName);
        }

    }
    
    private class PlantStockSubscription : IAggregateSubscription<PlantInfo, PlantStock>
    {
        public IEnumerable<EventSubscriptionBase<PlantInfo, PlantStock>> Subscriptions => new EventSubscriptionBase<PlantInfo, PlantStock>[]
        {
            new EventSubscription<PlantInfo, PlantStock, StockAddedEvent>(
                new AggregateLoadingTranspose<PlantInfo, StockAddedEvent>(
                    _ => InfoId,
                    (oldEvents, info) =>
                        oldEvents.Select(added => info.TransposeSubscribedEvent(added)))
                )
        };
    }

}
