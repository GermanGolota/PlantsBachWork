namespace Plants.Aggregates;

public record AddToStockCommand(CommandMetadata Metadata, PlantInformation Plant, DateTime CreatedTime, byte[][] Pictures) : Command(Metadata);
public record StockAddedEvent(EventMetadata Metadata, PlantInformation Plant, DateTime CreatedTime, Picture[] Pictures, string CaretakerUsername) : Event(Metadata);

public record EditStockItemCommand(CommandMetadata Metadata, PlantInformation Plant, byte[][] NewPictures, Guid[] RemovedPictureIds) : Command(Metadata);
public record StockEdditedEvent(EventMetadata Metadata, PlantInformation Plant, Picture[] NewPictures, Guid[] RemovedPictureIds) : Event(Metadata);

public record PostStockItemCommand(CommandMetadata Metadata, decimal Price) : Command(Metadata);
public record StockItemPostedEvent(EventMetadata Metadata, string SellerUsername, decimal Price, string GroupName) : Event(Metadata);

public record PlantInformation(
    string PlantName, string Description, string[] RegionNames,
    string SoilName, string GroupName
    );