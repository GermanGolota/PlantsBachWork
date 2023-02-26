using Humanizer;

namespace Plants.Aggregates;

public record AddToStockCommand(CommandMetadata Metadata, PlantInformation Plant, DateTime CreatedTime, byte[][] Pictures) : Command(Metadata);
public record StockAddedEvent(EventMetadata Metadata, PlantInformation Plant, DateTime CreatedTime, Picture[] Pictures, string CaretakerUsername) : Event(Metadata);

public record EditStockItemCommand(CommandMetadata Metadata, PlantInformation Plant, byte[][] NewPictures, Guid[] RemovedPictureIds) : Command(Metadata);
public record StockEdditedEvent(EventMetadata Metadata, PlantInformation Plant, Picture[] NewPictures, Guid[] RemovedPictureIds) : Event(Metadata);

public record PostStockItemCommand(CommandMetadata Metadata, decimal Price) : Command(Metadata);
public record StockItemPostedEvent(EventMetadata Metadata, string SellerUsername, decimal Price, string[] GroupNames) : Event(Metadata);

public record PlantInformation(
    string PlantName, string Description, string[] RegionNames,
    string[] SoilNames, string[] GroupNames
    );


// Queries

public record GetStockItems(PlantStockParams Params, QueryOptions Options) : IRequest<IEnumerable<StockViewResultItem>>;
public record PlantStockParams(bool IsMine) : ISearchParams;
public record StockViewResultItem(Guid Id, string PlantName, string Description, bool IsMine);


public record GetStockItem(Guid StockId) : IRequest<PlantViewResultItem?>;
public record PlantViewResultItem(string PlantName, string Description, string[] GroupNames,
    string[] SoilNames, Picture[] Images, string[] RegionNames, DateTime Created)
{
    public string CreatedHumanDate => Created.Humanize();
    public string CreatedDate => Created.ToShortDateString();
}


public record PreparedPostResultItem(
    Guid Id, string PlantName, string Description, string[] SoilNames,
    string[] RegionNames, string[] GroupNames, DateTime Created,
    string SellerName, string SellerPhone, long SellerCared, long SellerSold, long SellerInstructions,
    long CareTakerCared, long CareTakerSold, long CareTakerInstructions, Picture[] Images)
{
    public string CreatedHumanDate => Created.Humanize();
    public string CreatedDate => Created.ToShortDateString();
}

public record GetPrepared(Guid StockId) : IRequest<PreparedPostResultItem?>;