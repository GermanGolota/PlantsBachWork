using Humanizer;

namespace Plants.Aggregates;

// Commands

public record RemovePostCommand(CommandMetadata Metadata) : Command(Metadata);
public record PostRemovedEvent(EventMetadata Metadata) : Event(Metadata);

public record OrderPostCommand(CommandMetadata Metadata, DeliveryAddress Address) : Command(Metadata);
public record PostOrderedEvent(EventMetadata Metadata, DeliveryAddress Address, string BuyerUsername) : Event(Metadata);

// Queries

public record SearchPosts(PlantPostParams Parameters, QueryOptions Options) : IRequest<IEnumerable<PostSearchViewResultItem>>;
public record GetPost(Guid PostId) : IRequest<PostViewResultItem?>;

// Types

public record DeliveryAddress(string City, long MailNumber);

public record PostViewResultItem(Guid Id, string PlantName, string Description, decimal Price,
    string[] SoilNames, string[] RegionNames, string[] GroupNames, DateTime Created,
    string SellerName, string SellerPhone, long SellerCared, long SellerSold, long SellerInstructions,
    long CareTakerCared, long CareTakerSold, long CareTakerInstructions, Picture[] Images
)
{
    public string CreatedHumanDate => Created.Humanize();
    public string CreatedDate => Created.ToShortDateString();
}

public record PlantPostParams(
    string? PlantName,
    decimal? LowerPrice,
    decimal? TopPrice,
    DateTime? LastDate,
    string[]? GroupNames,
    string[]? RegionNames,
    string[]? SoilNames) : ISearchParams;

public record PostSearchViewResultItem(Guid Id, string PlantName, string Description, Picture[] Images, double Price);
