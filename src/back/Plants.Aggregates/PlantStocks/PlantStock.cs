namespace Plants.Aggregates.PlantStocks;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
public class PlantStock : AggregateBase, IDomainCommandHandler<AddToStockCommand>, IEventHandler<StockAddedEvent>
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
    //TODO: Think about it
    public byte[][] Pictures { get; private set; }

    public CommandForbidden? ShouldForbid(AddToStockCommand command, IUserIdentity userIdentity) =>
         userIdentity.HasRole(Producer)
        .And(this.RequireNew);

    public IEnumerable<Event> Handle(AddToStockCommand command) =>
         new[]
        {
            new StockAddedEvent(EventFactory.Shared.Create<StockAddedEvent>(command), command.Plant)
        };

    public void Handle(StockAddedEvent @event)
    {
        var plant = @event.Plant;
        PlantName = plant.PlantName;
        Description = plant.Description;
        Regions = plant.RegionNames;
        Group = plant.GroupName;
        Soil = plant.SoilName;
        Created = plant.Created;
        Pictures = plant.Pictures;
    }
}

public record AddToStockCommand(CommandMetadata Metadata, PlantStockDto Plant) : Command(Metadata);
public record StockAddedEvent(EventMetadata Metadata, PlantStockDto Plant) : Event(Metadata);
public record PlantStockDto(
    string PlantName, string Description, string[] RegionNames,
    string SoilName, string GroupName, DateTime Created, byte[][] Pictures
    );