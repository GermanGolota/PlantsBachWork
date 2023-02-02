using Plants.Domain.Aggregate;

namespace Plants.Aggregates.PlantStocks;

public record AddToStockCommand(CommandMetadata Metadata, PlantInformation Plant, DateTime CreatedTime, byte[][] Pictures) : Command(Metadata);
public record StockAddedEvent(EventMetadata Metadata, PlantInformation Plant, DateTime CreatedTime, string[] PictureUrls, string CaretakerUsername) : Event(Metadata);

public record EditStockItemCommand(CommandMetadata Metadata, PlantInformation Plant, byte[][] NewPictures, string[] RemovedPictureUrls) : Command(Metadata);
public record StockEdditedEvent(EventMetadata Metadata, PlantInformation Plant, string[] NewPictureUrls, string[] RemovedPictureUrls) : Event(Metadata);

public record PostStockItemCommand(CommandMetadata Metadata, decimal Price) : Command(Metadata);
public record StockItemPostedEvent(EventMetadata Metadata, string SellerUsername, decimal Price, string GroupName) : Event(Metadata);

public record PlantInformation(
    string PlantName, string Description, string[] RegionNames,
    string SoilName, string GroupName
    );