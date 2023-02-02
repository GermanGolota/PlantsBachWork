namespace Plants.Aggregates;

public record RemovePostCommand(CommandMetadata Metadata) : Command(Metadata);
public record PostRemovedEvent(EventMetadata Metadata) : Event(Metadata);

public record OrderPostCommand(CommandMetadata Metadata, DeliveryAddress Address) : Command(Metadata);
public record PostOrderedEvent(EventMetadata Metadata, DeliveryAddress Address, string BuyerUsername) : Event(Metadata);

public record DeliveryAddress(string City, long MailNumber);