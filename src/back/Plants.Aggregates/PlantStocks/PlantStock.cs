namespace Plants.Aggregates.PlantStocks;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
public class PlantStock : AggregateBase, IEventHandler<StockAddedEvent>, IEventHandler<StockEdditedEvent>, IDomainCommandHandler<PostStockItemCommand>
{
    public PlantStock(Guid id) : base(id)
    {
    }

    public PlantInformation Information { get; private set; }
    public string[] PictureUrls { get; private set; }
    public string CaretakerUsername { get; private set; }
    public DateTime CreatedTime { get; private set; }
    public bool BeenPosted { get; private set; } = false;

    public void Handle(StockAddedEvent @event)
    {
        Information = @event.Plant;
        PictureUrls = @event.PictureUrls;
        CaretakerUsername = @event.CaretakerUsername;
        CreatedTime = @event.CreatedTime;
    }

    public void Handle(StockEdditedEvent @event)
    {
        Information = @event.Plant;
        PictureUrls = PictureUrls.Except(@event.RemovedPictureUrls).Union(@event.NewPictureUrls).ToArray();
    }

    public CommandForbidden? ShouldForbid(PostStockItemCommand command, IUserIdentity userIdentity) =>
        userIdentity.HasAnyRoles(Producer, Manager).And(BeenPosted.ToForbidden("Cannot post again"));

    public IEnumerable<Event> Handle(PostStockItemCommand command)
    {
        BeenPosted = true;
        return new[]
        {
            new PostCreatedEvent(EventFactory.Shared.Create<PostCreatedEvent>(command), Id, command.Price)
        };
    }

}

public record AddToStockCommand(CommandMetadata Metadata, PlantInformation Plant, DateTime CreatedTime, byte[][] Pictures) : Command(Metadata);
public record StockAddedEvent(EventMetadata Metadata, PlantInformation Plant, DateTime CreatedTime, string[] PictureUrls, string CaretakerUsername) : Event(Metadata);

public record EditStockItemCommand(CommandMetadata Metadata, PlantInformation Plant, byte[][] NewPictures, string[] RemovedPictureUrls) : Command(Metadata);
public record StockEdditedEvent(EventMetadata Metadata, PlantInformation Plant, string[] NewPictureUrls, string[] RemovedPictureUrls) : Event(Metadata);

public record PostStockItemCommand(CommandMetadata Metadata, decimal Price) : Command(Metadata);
public record PostCreatedEvent(EventMetadata Metadata, Guid StockId, decimal Price) : Event(Metadata);

public record PlantInformation(
    string PlantName, string Description, string[] RegionNames,
    string SoilName, string GroupName
    );


