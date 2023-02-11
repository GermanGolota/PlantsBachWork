namespace Plants.Aggregates;

[Allow(Consumer, Read)]
[Allow(Consumer, Write)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Manager, Read)]
[Allow(Manager, Write)]
public class PlantPost : AggregateBase, IEventHandler<StockItemPostedEvent>,
    IDomainCommandHandler<RemovePostCommand>, IEventHandler<PostRemovedEvent>,
    IDomainCommandHandler<OrderPostCommand>, IEventHandler<PostOrderedEvent>,
    IEventHandler<RejectedOrderEvent>
{
    public PlantPost(Guid id) : base(id)
    {
    }

    public decimal Price { get; private set; }
    public PlantStock Stock { get; private set; }
    public User Seller { get; private set; }
    public bool IsRemoved { get; private set; }
    public bool IsOrdered { get; private set; }

    public void Handle(StockItemPostedEvent @event)
    {
        Metadata.Referenced.Add(new(@event.Metadata.Aggregate.Id, nameof(PlantStock)));
        Metadata.Referenced.Add(new(@event.SellerUsername.ToGuid(), nameof(User)));
        IsRemoved = false;
        IsOrdered = false;
        Price = @event.Price;
    }

    public CommandForbidden? ShouldForbid(RemovePostCommand command, IUserIdentity user) =>
        (user.HasRole(Manager).Or(user.HasRole(Producer).And(IsSeller(user))))
        .And(IsNotRemoved)
        .And(IsNotOrdered);

    private CommandForbidden? IsNotRemoved() =>
        (IsRemoved is false).ToForbidden("Already removed");

    private CommandForbidden? IsNotOrdered() =>
        (IsOrdered is false).ToForbidden("Already ordered");

    private CommandForbidden? IsSeller(IUserIdentity user) =>
        (user.UserName == Seller.Login).ToForbidden("Cannot remove somebody elses post");

    public IEnumerable<Event> Handle(RemovePostCommand command) =>
        new[]
        {
            new PostRemovedEvent(EventFactory.Shared.Create<PostRemovedEvent>(command))
        };

    public void Handle(PostRemovedEvent @event)
    {
        IsRemoved = true;
    }

    public CommandForbidden? ShouldForbid(OrderPostCommand command, IUserIdentity user) =>
        user.HasAnyRoles(Manager, Consumer).And(IsNotRemoved).And(IsNotOrdered);

    public IEnumerable<Event> Handle(OrderPostCommand command) =>
        new[]
        {
            new PostOrderedEvent(EventFactory.Shared.Create<PostOrderedEvent>(command), command.Address, command.Metadata.UserName)
        };

    public void Handle(PostOrderedEvent @event)
    {
        IsOrdered = true;
    }

    public void Handle(RejectedOrderEvent @event)
    {
        IsOrdered = false;
    }

    private class PlantStockSubscription : IAggregateSubscription<PlantPost, PlantStock>
    {
        public IEnumerable<EventSubscriptionBase<PlantPost, PlantStock>> Subscriptions => new[]
        {
            new EventSubscription<PlantPost, PlantStock, StockItemPostedEvent>(
                new(@event => @event.Metadata.Aggregate.Id,
                    (events, post) => events.Select(@event => post.TransposeSubscribedEvent(@event))))
        };
    }

    private class PlantOrderSubscription : IAggregateSubscription<PlantPost, PlantOrder>
    {
        public IEnumerable<EventSubscriptionBase<PlantPost, PlantOrder>> Subscriptions => new[]
        {
            new EventSubscription<PlantPost, PlantOrder, RejectedOrderEvent>(
                new(@event => @event.Metadata.Aggregate.Id,
                    (events, post) => events.Select(@event => post.TransposeSubscribedEvent(@event))))
        };
    }
}
