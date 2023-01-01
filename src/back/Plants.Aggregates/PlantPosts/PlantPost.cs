using Plants.Aggregates.PlantStocks;
using Plants.Aggregates.Users;

namespace Plants.Aggregates.PlantPosts;

[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Consumer, Read)]
public class PlantPost : AggregateBase, IEventHandler<StockItemPostedEvent>, IDomainCommandHandler<RemovePostCommand>, IEventHandler<PostRemovedEvent>
{
    public PlantPost(Guid id) : base(id)
    {
    }

    public decimal Price { get; private set; }
    public PlantStock Stock { get; private set; }
    public User Seller { get; private set; }
    public bool IsRemoved { get; private set; }

    public void Handle(StockItemPostedEvent @event)
    {
        Referenced.Add(new(@event.Metadata.Aggregate.Id, nameof(PlantStock)));
        Referenced.Add(new(@event.SellerUsername.ToGuid(), nameof(User)));
        IsRemoved = false;
        Price = @event.Price;
    }

    public CommandForbidden? ShouldForbid(RemovePostCommand command, IUserIdentity user) =>
        IsNotRemoved().And(
            user.HasRole(Manager).Or(user.HasRole(Producer).And(IsSeller(user))));

    private CommandForbidden? IsNotRemoved() =>
        (IsRemoved is false).ToForbidden("Already removed");

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

    private class PlantStockSubscription : IAggregateSubscription<PlantPost, PlantStock>
    {
        public IEnumerable<EventSubscriptionBase<PlantPost, PlantStock>> Subscriptions => new[]
        {
            new EventSubscription<PlantPost, PlantStock, StockItemPostedEvent>(
                new(@event => @event.Metadata.Aggregate.Id,
                    (events, post) => events.Select(@event => post.TransposeSubscribedEvent(@event))))
        };
    }
}

public record RemovePostCommand(CommandMetadata Metadata) : Command(Metadata);
public record PostRemovedEvent(EventMetadata Metadata) : Event(Metadata);