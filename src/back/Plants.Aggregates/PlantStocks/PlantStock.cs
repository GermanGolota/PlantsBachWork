namespace Plants.Aggregates.PlantStocks;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
public class PlantStock : AggregateBase, IEventHandler<StockAddedEvent>
{
    public PlantStock(Guid id) : base(id)
    {
    }

    public string PlantName { get; private set; }
    public string Description { get; private set; }
    public string[] Regions { get; private set; }
    public string Group { get; private set; }
    public string Soil { get; private set; }
    public DateTime Created { get; private set; }
    public string[] PictureUrls { get; private set; }


    public void Handle(StockAddedEvent @event)
    {
        var plant = @event.Plant;
        PlantName = plant.PlantName;
        Description = plant.Description;
        Regions = plant.RegionNames;
        Group = plant.GroupName;
        Soil = plant.SoilName;
        Created = plant.Created;
        PictureUrls = @event.PictureUrls;
    }
}

public record AddToStockCommand(CommandMetadata Metadata, PlantStockDto Plant, byte[][] Pictures) : Command(Metadata);
public record StockAddedEvent(EventMetadata Metadata, PlantStockDto Plant, string[] PictureUrls) : Event(Metadata);
public record PlantStockDto(
    string PlantName, string Description, string[] RegionNames,
    string SoilName, string GroupName, DateTime Created
    );