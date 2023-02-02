using Plants.Aggregates.PlantStocks;
using Plants.Domain.Aggregate;
using System.Text;

namespace Plants.Aggregates.PlantInfos;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Manager, Read)]
[Allow(Manager, Write)]
public class PlantInfo : AggregateBase, IEventHandler<StockAddedEvent>, IEventHandler<StockEdditedEvent>
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

    public Dictionary<long, string> GroupNames { get; private set; } = new();
    public Dictionary<long, string> RegionNames { get; private set; } = new();
    public Dictionary<long, string> SoilNames { get; private set; } = new();
    public Dictionary<long, string> PlantImagePaths { get; private set; } = new();

    public void Handle(StockAddedEvent @event)
    {
        var plant = @event.Plant;
        GroupNames.CacheTransformation(plant.GroupName, ToLong);
        SoilNames.CacheTransformation(plant.SoilName, ToLong);
        foreach (var regionName in plant.RegionNames)
        {
            RegionNames.CacheTransformation(regionName, ToLong);
        }

        foreach (var path in @event.PictureUrls)
        {
            PlantImagePaths.CacheTransformation(path, ToLong);
        }
    }

    private long ToLong(string str)
    {
        var initialBytes = Encoding.UTF8.GetBytes(str);
        var bytes = initialBytes.Reverse().Take(8).ToArray();
        if (bytes.Length < 8)
        {
            var newBytes = new byte[8];
            bytes.CopyTo(newBytes, 0);
            for (int i = bytes.Length; i < 8; i++)
            {
                newBytes[i] = 0;
            }
            bytes = newBytes;
        }
        return BitConverter.ToInt64(bytes);
    }

    public void Handle(StockEdditedEvent @event)
    {
        if (@event.RemovedPictureUrls.Any())
        {
            var keys = PlantImagePaths.Where(_ => _.Value.In(@event.RemovedPictureUrls)).Select(_ => _.Key);
            foreach (var key in keys)
            {
                PlantImagePaths.Remove(key);
            }
        }

        foreach (var image in @event.NewPictureUrls)
        {
            PlantImagePaths.CacheTransformation(image, ToLong);
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
                ),
            new EventSubscription<PlantInfo, PlantStock, StockEdditedEvent>(
                new AggregateLoadingTranspose<PlantInfo, StockEdditedEvent>(
                    _ => InfoId,
                    (oldEvents, info) =>
                        oldEvents.Select(added => info.TransposeSubscribedEvent(added)))
                )
        };
    }

}
