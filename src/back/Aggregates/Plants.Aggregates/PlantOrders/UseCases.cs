namespace Plants.Aggregates;

// Commands

public record StartOrderDeliveryCommand(CommandMetadata Metadata, string TrackingNumber) : Command(Metadata);
public record OrderDeliveryStartedEvent(EventMetadata Metadata, string TrackingNumber) : Event(Metadata);

public record RejectOrderCommand(CommandMetadata Metadata) : Command(Metadata);
public record RejectedOrderEvent(EventMetadata Metadata) : Event(Metadata);

public record ConfirmDeliveryCommand(CommandMetadata Metadata) : Command(Metadata);
public record DeliveryConfirmedEvent(EventMetadata Metadata, string SellerUsername, string[] GroupNames, decimal Price) : Event(Metadata);

// Queries


public record SearchOrders(PlantOrderParams Parameters, QueryOptions Options) : IRequest<IEnumerable<OrdersViewResultItem>>;

public record PlantOrderParams(bool OnlyMine) : ISearchParams;

public record OrdersViewResultItem(
    int Status, Guid PostId, string City,
    long MailNumber, string SellerName, string SellerContact,
    decimal Price, string? DeliveryTrackingNumber, Picture[] Images,
    DateTime Ordered, DateTime? DeliveryStarted, DateTime? Shipped)
{
    public string OrderedDate => Ordered.ToShortDateString();
    public string? DeliveryStartedDate => DeliveryStarted?.ToShortDateString();
    public string? ShippedDate => Shipped?.ToShortDateString();
}
