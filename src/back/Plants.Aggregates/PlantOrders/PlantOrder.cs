using Plants.Aggregates.PlantPosts;
using Plants.Aggregates.Users;

namespace Plants.Aggregates.PlantOrders;

[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
public class PlantOrder : AggregateBase, IEventHandler<PostOrderedEvent>,
    IDomainCommandHandler<StartOrderDeliveryCommand>, IEventHandler<OrderDeliveryStartedEvent>,
    IDomainCommandHandler<RejectOrderCommand>, IEventHandler<RejectedOrderEvent>,
    IDomainCommandHandler<ConfirmDeliveryCommand>, IEventHandler<DeliveryConfirmedEvent>
{
    public PlantOrder(Guid id) : base(id)
    {
    }

    public DeliveryAddress Address { get; private set; }
    public PlantPost Post { get; private set; }
    public User Buyer { get; private set; }

    public string TrackingNumber { get; private set; }

    public OrderStatus Status { get; private set; }

    public void Handle(PostOrderedEvent @event)
    {
        Address = @event.Address;
        Status = OrderStatus.Created;
        Referenced.Add(new(@event.Metadata.Aggregate.Id, nameof(PlantPost)));
        Referenced.Add(new(@event.BuyerUsername.ToGuid(), nameof(User)));
    }

    public CommandForbidden? ShouldForbid(StartOrderDeliveryCommand command, IUserIdentity user) =>
        IsSellerOrManager(user).And(StatusIs(OrderStatus.Created));

    private CommandForbidden? StatusIs(OrderStatus expectedStatus)
    {
        return (Status == expectedStatus).ToForbidden($"The order is already in status '{Status}'");
    }

    private CommandForbidden? IsSellerOrManager(IUserIdentity user) =>
        user.HasRole(Manager).Or(user.HasRole(Producer).And(IsSeller(user)));

    private CommandForbidden? IsSeller(IUserIdentity user) =>
        (Post.Seller.Login == user.UserName).ToForbidden("Cannot confirm an order of some other seller");

    private CommandForbidden? IsBuyer(IUserIdentity user) =>
        (Buyer.Login == user.UserName).ToForbidden("Cannot confirm an order of some other buyer");

    public IEnumerable<Event> Handle(StartOrderDeliveryCommand command) =>
        new[]
        {
            new OrderDeliveryStartedEvent(EventFactory.Shared.Create<OrderDeliveryStartedEvent>(command), command.TrackingNumber)
        };

    public void Handle(OrderDeliveryStartedEvent @event)
    {
        Status = OrderStatus.Delivering;
    }

    public void Handle(RejectedOrderEvent @event)
    {
        Status = OrderStatus.Rejected;
    }

    public CommandForbidden? ShouldForbid(RejectOrderCommand command, IUserIdentity user) =>
        IsSellerOrManager(user).And(StatusIs(OrderStatus.Created));

    public IEnumerable<Event> Handle(RejectOrderCommand command) =>
        new[]
        {
            new RejectedOrderEvent(EventFactory.Shared.Create<RejectedOrderEvent>(command))
        };

    public CommandForbidden? ShouldForbid(ConfirmDeliveryCommand command, IUserIdentity user) =>
        user.HasRole(Manager).Or(user.HasRole(Producer).And(IsBuyer(user)))
        .And(StatusIs(OrderStatus.Delivering));

    public IEnumerable<Event> Handle(ConfirmDeliveryCommand command) => new[]
    {
        new DeliveryConfirmedEvent(EventFactory.Shared.Create<DeliveryConfirmedEvent>(command))
    };

    public void Handle(DeliveryConfirmedEvent @event)
    {
        Status = OrderStatus.Delivered;
    }

    private class PlantPostSubscription : IAggregateSubscription<PlantOrder, PlantPost>
    {
        public IEnumerable<EventSubscriptionBase<PlantOrder, PlantPost>> Subscriptions => new[]
        {
            new EventSubscription<PlantOrder, PlantPost, PostOrderedEvent>(new(
                @event => @event.Metadata.Aggregate.Id,
                (events, post) => events.Select(@event => post.TransposeSubscribedEvent(@event))))
        };
    }
}

public enum OrderStatus
{
    Created = 0, Delivering = 1, Delivered = 2, Rejected = 3
}

public record StartOrderDeliveryCommand(CommandMetadata Metadata, string TrackingNumber) : Command(Metadata);
public record OrderDeliveryStartedEvent(EventMetadata Metadata, string TrackingNumber) : Event(Metadata);

public record RejectOrderCommand(CommandMetadata Metadata) : Command(Metadata);
public record RejectedOrderEvent(EventMetadata Metadata) : Event(Metadata);

public record ConfirmDeliveryCommand(CommandMetadata Metadata) : Command(Metadata);
public record DeliveryConfirmedEvent(EventMetadata Metadata) : Event(Metadata);