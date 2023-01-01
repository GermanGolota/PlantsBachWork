namespace Plants.Aggregates.PlantStocks;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
public class PlantStock : AggregateBase, IEventHandler<StockAddedEvent>
{
    public PlantStock(Guid id) : base(id)
    {
    }

    public PlantInformation Information { get; private set; }
    public string[] PictureUrls { get; private set; }
    public string CaretakerUsername { get; private set; }
    public DateTime CreatedTime { get; private set; }

    public void Handle(StockAddedEvent @event)
    {
        Information = @event.Plant;
        PictureUrls = @event.PictureUrls;
        CaretakerUsername = @event.CaretakerUsername;
        CreatedTime = @event.CreatedTime;
    }
}

public record AddToStockCommand(CommandMetadata Metadata, PlantInformation Plant, DateTime CreatedTime, byte[][] Pictures) : Command(Metadata);
public record StockAddedEvent(EventMetadata Metadata, PlantInformation Plant, DateTime CreatedTime, string[] PictureUrls, string CaretakerUsername) : Event(Metadata);

public record EditStockCommand(CommandMetadata Metadata) : Command(Metadata);
public record StockAdditedEvent(CommandMetadata Metadata) : Command(Metadata);

public record PlantInformation(
    string PlantName, string Description, string[] RegionNames,
    string SoilName, string GroupName
    );