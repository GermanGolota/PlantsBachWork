using Plants.Aggregates.PlantStocks;

namespace Plants.Aggregates.PlantInfos;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
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

    public Dictionary<Guid, string> GroupNames { get; set; } = new();
    public Dictionary<Guid, string> RegionNames { get; set; } = new();
    public Dictionary<Guid, string> SoilNames { get; set; } = new();

    public void Handle(StockAddedEvent @event)
    {
        var plant = @event.Plant;
        GroupNames.CacheTransformation(plant.GroupName, StringExtensions.ToGuid);
        SoilNames.CacheTransformation(plant.SoilName, StringExtensions.ToGuid);
        foreach (var regionName in plant.RegionNames)
        {
            RegionNames.CacheTransformation(regionName, StringExtensions.ToGuid);
        }
    }

    private class PlantInfoSubscriptions : IAggregateSubscription<PlantInfo, PlantStock>
    {
        public IEnumerable<EventSubscriptionBase<PlantInfo, PlantStock>> Subscriptions => new[]
        {
            new EventSubscription<PlantInfo, PlantStock, StockAddedEvent>(
                new AllEvents(),
                new AggregateLoadingTranspose<PlantInfo, StockAddedEvent>(
                    _ => InfoId,
                    (oldEvents, info) =>
                        oldEvents.Select(added => info.TransposeSubscribedEvent(added)))
                )
        };
    }

}
