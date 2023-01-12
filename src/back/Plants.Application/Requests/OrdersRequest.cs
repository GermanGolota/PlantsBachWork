using MediatR;

namespace Plants.Application.Requests;

public record OrdersRequest(bool OnlyMine) : IRequest<OrdersResult>;

public record OrdersResult(List<OrdersResultItem> Items);

public record OrdersResultItem(
    int Status, long PostId, string City,
    long MailNumber, string SellerName, string SellerContact,
    decimal Price, string? DeliveryTrackingNumber, long[] Images)
{
    //decoder
    public OrdersResultItem() : this(0, 0, "", 
        0, "", "", 0, null, Array.Empty<long>())
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