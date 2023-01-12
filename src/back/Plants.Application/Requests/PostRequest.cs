using Humanizer;
using MediatR;

namespace Plants.Application.Requests;

public record PostRequest(long PostId) : IRequest<PostResult>;

public record PostResult(bool Exists, PostResultItem Item)
{
    public PostResult() : this (false, null)
    {

    }

    public PostResult(PostResultItem item) : this(true, item)
    {

    }
}

public class PostResultItem
{
    public long Id { get; set; }
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
    public long[] Images { get; set; }
    public PostResultItem()
    {

    }

    public PostResultItem(long id, string plantName, string description, decimal price,
        string soilName, string[] regions, string groupName, DateTime created, string sellerName,
        string sellerPhone, long sellerCared, long sellerSold, long sellerInstructions,
        long careTakerCared, long careTakerSold, long careTakerInstructions, long[] images)
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
