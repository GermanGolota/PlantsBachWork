using Plants.Aggregates.PlantStocks;
using Plants.Aggregates.Users;

namespace Plants.Aggregates.PlantPosts;

[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Consumer, Read)]
public class PlantPost : AggregateBase, IEventHandler<StockItemPostedEvent>
{
    public PlantPost(Guid id) : base(id)
    {
    }

    public decimal Price { get; private set; }
    public PlantStock Stock { get; private set; }
    public User Seller { get; private set; }

    public void Handle(StockItemPostedEvent @event)
    {
        Referenced.Add(new(@event.Metadata.Aggregate.Id, nameof(PlantStock)));
        Referenced.Add(new(@event.SellerUsername.ToGuid(), nameof(User)));
        Price = @event.Price;
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
