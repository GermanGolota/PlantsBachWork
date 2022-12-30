using Plants.Aggregates.PlantStocks;
using Plants.Aggregates.Users;

namespace Plants.Aggregates.UserStats;

public class UserStat : AggregateBase, IEventHandler<UserCreatedEvent>, IEventHandler<StockAddedEvent>
{
    public UserStat(Guid id) : base(id)
    {
    }

    public string Username { get; private set; }
    public long PlantsCaredFor { get; private set; }

    public void Handle(UserCreatedEvent @event)
    {
        Username = @event.Data.Login;
        PlantsCaredFor = 0;
    }

    public void Handle(StockAddedEvent @event)
    {
        PlantsCaredFor++;
    }

    private class UserStatUserSubscription : IAggregateSubscription<UserStat, User>
    {
        public IEnumerable<EventSubscriptionBase<UserStat, User>> Subscriptions => new List<EventSubscriptionBase<UserStat, User>>()
        {
             new EventSubscription<UserStat, User, UserCreatedEvent>(
                new AllEvents(),
                new AggregateLoadingTranspose<UserStat, UserCreatedEvent>(
                    @event => @event.Data.Login.ToGuid(),
                    (events, stats) =>
                        events.Select(@event => stats.TransposeSubscribedEvent(@event)))
                )
        };
    }

    private class UserStatStockSubscription : IAggregateSubscription<UserStat, PlantStock>
    {
        public IEnumerable<EventSubscriptionBase<UserStat, PlantStock>> Subscriptions => new List<EventSubscriptionBase<UserStat, PlantStock>>()
        {
             new EventSubscription<UserStat, PlantStock, StockAddedEvent>(
                new AllEvents(),
                new AggregateLoadingTranspose<UserStat, StockAddedEvent>(
                    @event => @event.CaretakerUsername.ToGuid(),
                    (events, stats) =>
                        events.Select(@event => stats.TransposeSubscribedEvent(@event)))
                )
        };
    }
}
