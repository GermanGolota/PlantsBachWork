using Plants.Aggregates.PlantInstructions;
using Plants.Aggregates.PlantStocks;
using System.Text;

namespace Plants.Aggregates.PlantInfos;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Manager, Read)]
[Allow(Manager, Write)]
public class PlantInfo : AggregateBase, IEventHandler<StockAddedEvent>, IEventHandler<InstructionCreatedEvent>
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
    public Dictionary<long, string> InstructionCoverImagePaths { get; private set; } = new();

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

    public void Handle(InstructionCreatedEvent @event)
    {
        InstructionCoverImagePaths.CacheTransformation(@event.CoverUrl, str => @event.InstructionId.ToLong());
    }

    private long ToLong(string str)
    {
        var initialBytes = Encoding.UTF8.GetBytes(str);
        var bytes = initialBytes.Reverse().Take(8).ToArray();
        if(bytes.Length < 8)
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

    private class PlantStockSubscription : IAggregateSubscription<PlantInfo, PlantStock>
    {
        public IEnumerable<EventSubscriptionBase<PlantInfo, PlantStock>> Subscriptions => new[]
        {
            new EventSubscription<PlantInfo, PlantStock, StockAddedEvent>(
                new AggregateLoadingTranspose<PlantInfo, StockAddedEvent>(
                    _ => InfoId,
                    (oldEvents, info) =>
                        oldEvents.Select(added => info.TransposeSubscribedEvent(added)))
                )
        };
    }

    private class PlantInstructionSubscription : IAggregateSubscription<PlantInfo, PlantInstruction>
    {
        public IEnumerable<EventSubscriptionBase<PlantInfo, PlantInstruction>> Subscriptions => new[]
        {
            EventSubscriptionFactory.CreateForwarded<PlantInfo, PlantInstruction, InstructionCreatedEvent>(_ => InfoId)
        };
    }

}
