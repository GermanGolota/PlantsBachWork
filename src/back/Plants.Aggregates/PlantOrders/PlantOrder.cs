using Plants.Aggregates.PlantPosts;
using Plants.Aggregates.Users;

namespace Plants.Aggregates.PlantOrders;

[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
public class PlantOrder : AggregateBase, IEventHandler<PostOrderedEvent>
{
    public PlantOrder(Guid id) : base(id)
    {
    }

    public DeliveryAddress Address { get; private set; }
    public PlantPost Post { get; private set; }
    public User Buyer { get; set; }

    public void Handle(PostOrderedEvent @event)
    {
        Address = @event.Address;
        Referenced.Add(new(@event.Metadata.Aggregate.Id, nameof(PlantPost)));
        Referenced.Add(new(@event.BuyerUsername.ToGuid(), nameof(User)));
    }

    private class PlantPostSubscription : IAggregateSubscription<PlantOrder, PlantPost>
    {
        public IEnumerable<EventSubscriptionBase<PlantOrder, PlantPost>> Subscriptions => new[]
        {
            new EventSubscription<PlantOrder, PlantPost, PostOrderedEvent>(new(
                @event => @event.Metadata.Id,
                (events, post) => events.Select(@event => post.TransposeSubscribedEvent(@event))))
        };
    }
}
