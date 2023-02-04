using Humanizer;

namespace Plants.Presentation;

//info
public record DictsResult2(Dictionary<string, string> Groups, Dictionary<string, string> Regions, Dictionary<string, string> Soils);

public record AddressResult(List<PersonAddress> Addresses);
public record PersonAddress(string City, long MailNumber);

//instructions

public record CreateInstructionCommandDto(long GroupId, string Text,
  string Title, string Description);

public record FindInstructionsResult2(List<FindInstructionsResultItem2> Items);
public record FindInstructionsResultItem2(Guid Id, string Title, string Description, bool HasCover)
{
    public FindInstructionsResultItem2() : this(Guid.NewGuid(), "", "", false)
    {
    }
}

public record FindInstructionsRequest(long GroupId, string? Title, string? Description);

public record GetInstructionResult2(bool Exists, GetInstructionResultItem2 Item);
public record GetInstructionResultItem2(Guid Id, string Title, string Description,
    string InstructionText, bool HasCover, string PlantGroupId)
{
    //decoder
    public GetInstructionResultItem2() : this(Guid.NewGuid(), "", "", "", false, "-1")
    {

    }
}

public record CreateInstructionResult2(Guid Id);

public record EditInstructionResult2(Guid InstructionId);

//orders

public record OrdersResult2(List<OrdersResultItem2> Items);

public record OrdersResultItem2(
    int Status, Guid PostId, string City,
    long MailNumber, string SellerName, string SellerContact,
    decimal Price, string? DeliveryTrackingNumber, string[] Images)
{
    //decoder
    public OrdersResultItem2() : this(0, Guid.NewGuid(), "",
        0, "", "", 0, null, Array.Empty<string>())
    {

    }

    private DateTime ordered;
    private DateTime? deliveryStarted;
    private DateTime? shipped;

    public DateTime Ordered
    {
        get => ordered;
        set
        {
            ordered = value;
            OrderedDate = ordered.ToShortDateString();
        }
    }

    public DateTime? DeliveryStarted
    {
        get => deliveryStarted;
        set
        {
            deliveryStarted = value;
            DeliveryStartedDate = value?.ToString();
        }
    }

    public DateTime? Shipped
    {
        get => shipped;
        set
        {
            shipped = value;
            ShippedDate = value?.ToString();
        }
    }

    public string OrderedDate { get; set; }
    public string? DeliveryStartedDate { get; set; }
    public string? ShippedDate { get; set; }
}

public record ConfirmDeliveryResult(bool Successfull);
public record StartDeliveryResult(bool Successfull);

public record RejectOrderResult(bool Success);

//plants

public record AddPlantDto(string Name, string Description, long[] Regions, long SoilId, long GroupId, DateTime Created);

public record EditPlantDto(string PlantName,
  string PlantDescription, long[] RegionIds, long SoilId, long GroupId, long[]? RemovedImages);

public record PlantResult2(bool Exists, PlantResultDto2? Item)
{
    public PlantResult2(PlantResultDto2 item) : this(true, item)
    {

    }

    public PlantResult2() : this(false, null)
    {

    }
}

public record PlantsResult2(List<PlantResultItem2> Items);
public record PlantResultItem2(Guid Id, string PlantName, string Description, bool IsMine);

public record PlantResultDto2(string PlantName, string Description, string GroupId,
    string SoilId, string[] Images, string[] Regions)
{
    private DateTime _created;
    public DateTime Created
    {
        get { return _created; }
        set
        {
            _created = value;
            CreatedHumanDate = value.Humanize();
            CreatedDate = value.ToShortDateString();
        }
    }
    public string CreatedHumanDate { get; set; }
    public string CreatedDate { get; set; }

}


public record PreparedPostResult2(bool Exists, PreparedPostResultItem2 Item)
{
    public PreparedPostResult2() : this(false, null)
    {

    }

    public PreparedPostResult2(PreparedPostResultItem2 item) : this(true, item)
    {

    }
}

public class PreparedPostResultItem2
{
    public Guid Id { get; set; }
    public string PlantName { get; set; }
    public string Description { get; set; }
    public string SoilName { get; set; }
    public string[] Regions { get; set; }
    public string GroupName { get; set; }
    private DateTime created;

    public DateTime Created
    {
        get { return created; }
        set
        {
            created = value;
            CreatedHumanDate = value.Humanize();
            CreatedDate = value.ToShortDateString();
        }
    }

    public string SellerName { get; set; }
    public string SellerPhone { get; set; }
    public long SellerCared { get; set; }
    public long SellerSold { get; set; }
    public long SellerInstructions { get; set; }
    public long CareTakerCared { get; set; }
    public long CareTakerSold { get; set; }
    public long CareTakerInstructions { get; set; }
    public string[] Images { get; set; }
    public PreparedPostResultItem2()
    {

    }

    public PreparedPostResultItem2(Guid id, string plantName, string description,
        string soilName, string[] regions, string groupName, DateTime created, string sellerName,
        string sellerPhone, long sellerCared, long sellerSold, long sellerInstructions,
        long careTakerCared, long careTakerSold, long careTakerInstructions, string[] images)
    {
        Id = id;
        PlantName = plantName;
        Description = description;
        SoilName = soilName;
        Regions = regions;
        GroupName = groupName;
        Created = created;
        SellerName = sellerName;
        SellerPhone = sellerPhone;
        SellerCared = sellerCared;
        SellerSold = sellerSold;
        SellerInstructions = sellerInstructions;
        CareTakerCared = careTakerCared;
        CareTakerSold = careTakerSold;
        CareTakerInstructions = careTakerInstructions;
        Images = images;
    }
    public string CreatedHumanDate { get; set; }
    public string CreatedDate { get; set; }
}

public record CreatePostResult(bool Successfull, string Message);

public record AddPlantResult2(Guid Id);

public record EditPlantResult(bool Success, string Message);


//post


public record PostResult2(bool Exists, PostResultItem2 Item)
{
    public PostResult2() : this(false, null)
    {

    }

    public PostResult2(PostResultItem2 item) : this(true, item)
    {

    }
}

public class PostResultItem2
{
    public Guid Id { get; set; }
    public string PlantName { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string SoilName { get; set; }
    public string[] Regions { get; set; }
    public string GroupName { get; set; }
    private DateTime created;

    public DateTime Created
    {
        get { return created; }
        set
        {
            created = value;
            CreatedHumanDate = value.Humanize();
            CreatedDate = value.ToShortDateString();
        }
    }

    public string SellerName { get; set; }
    public string SellerPhone { get; set; }
    public long SellerCared { get; set; }
    public long SellerSold { get; set; }
    public long SellerInstructions { get; set; }
    public long CareTakerCared { get; set; }
    public long CareTakerSold { get; set; }
    public long CareTakerInstructions { get; set; }
    public string[] Images { get; set; }
    public PostResultItem2()
    {

    }

    public PostResultItem2(Guid id, string plantName, string description, decimal price,
        string soilName, string[] regions, string groupName, DateTime created, string sellerName,
        string sellerPhone, long sellerCared, long sellerSold, long sellerInstructions,
        long careTakerCared, long careTakerSold, long careTakerInstructions, string[] images)
    {
        Id = id;
        PlantName = plantName;
        Description = description;
        Price = price;
        SoilName = soilName;
        Regions = regions;
        GroupName = groupName;
        Created = created;
        SellerName = sellerName;
        SellerPhone = sellerPhone;
        SellerCared = sellerCared;
        SellerSold = sellerSold;
        SellerInstructions = sellerInstructions;
        CareTakerCared = careTakerCared;
        CareTakerSold = careTakerSold;
        CareTakerInstructions = careTakerInstructions;
        Images = images;
    }
    public string CreatedHumanDate { get; set; }
    public string CreatedDate { get; set; }
}

public record PlaceOrderResult(bool Successfull, string Message);

public record DeletePostResult(bool Deleted);

//search

public record SearchResult2(List<SearchResultItem2> Items);
public record SearchResultItem2(Guid Id, string PlantName, string Description, string[] ImageIds, double Price)
{
    //used by converter
    public SearchResultItem2() : this(Guid.Empty, "", "", null, 0)
    {

    }
}

public record SearchRequest(string? PlantName,
    decimal? LowerPrice,
    decimal? TopPrice,
    DateTime? LastDate,
    long[]? GroupIds,
    long[]? RegionIds,
    long[]? SoilIds);


// stats

public record FinancialStatsResult2(IEnumerable<GroupFinancialStats2> Groups);
public class GroupFinancialStats2
{
    public decimal Income { get; set; }
    public string GroupId { get; set; }
    public string GroupName { get; set; }
    public long SoldCount { get; set; }
    public double PercentSold { get; set; }
}

public record TotalStatsResult2(IEnumerable<GroupTotalStats2> Groups);
public record GroupTotalStats2(string GroupId, string GroupName, decimal Income, long Instructions, long Popularity);

// user

public record FindUsersResult(List<FindUsersResultItem> Items);
public record FindUsersResultItem(string FullName, string Mobile, string Login)
{
    //for converter
    public FindUsersResultItem() : this("", "", "")
    {

    }
    private string[] roles;

    public string[] Roles
    {
        get => roles;
        set
        {
            roles = value;
            RoleCodes = value.Select(To).ToArray();

        }
    }

    private static UserRole To(string role)
    {
        return role switch
        {
            "consumer" => UserRole.Consumer,
            "producer" => UserRole.Producer,
            "manager" => UserRole.Manager,
            _ => throw new ArgumentException("Bad role name", role)
        };
    }

    public UserRole[] RoleCodes { get; set; }
}

public record AlterRoleResult(bool Successfull);

public record CreateUserResult(bool Success, string Message);

public record CreateUserCommandView(string Login, List<UserRole> Roles, string Email, string? Language,
    string FirstName, string LastName, string PhoneNumber);

public record ChangePasswordResult(bool Success, string Message)
{
    public ChangePasswordResult() : this(true, "")
    {

    }

    public ChangePasswordResult(string msg) : this(false, msg)
    {

    }
}

public record PasswordChangeDto(string Password);

