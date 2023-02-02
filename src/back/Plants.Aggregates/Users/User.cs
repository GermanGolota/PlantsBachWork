using Plants.Aggregates.PlantInstructions;
using Plants.Aggregates.PlantOrders;
using Plants.Aggregates.PlantPosts;
using Plants.Aggregates.PlantStocks;
using Plants.Shared.Model;

namespace Plants.Aggregates.Users;

[Allow(Consumer, Read)]
[Allow(Consumer, Write)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Manager, Read)]
[Allow(Manager, Write)]
public class User : AggregateBase,
    IEventHandler<UserCreatedEvent>,
    IEventHandler<RoleChangedEvent>,
    IEventHandler<StockAddedEvent>,
    IEventHandler<PostOrderedEvent>,
    IEventHandler<DeliveryConfirmedEvent>,
    IEventHandler<InstructionCreatedEvent>
{
    public User(Guid id) : base(id)
    {
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string FullName { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Login { get; private set; }
    public UserRole[] Roles { get; private set; }

    public long PlantsCared { get; private set; } = 0;
    public long PlantsSold { get; private set; } = 0;
    public long InstructionCreated { get; private set; } = 0;

    public HashSet<DeliveryAddress> UsedAdresses { get; private set; }

    public void Handle(UserCreatedEvent @event)
    {
        var user = @event.Data;
        FirstName = user.FirstName;
        LastName = user.LastName;
        FullName = user.FirstName + " " + user.LastName;
        PhoneNumber = user.PhoneNumber;
        Login = user.Login;
        Roles = user.Roles;
        UsedAdresses = new();
    }

    public void Handle(RoleChangedEvent @event)
    {
        if (Roles.Contains(@event.Role))
        {
            Roles = Roles.Where(x => x != @event.Role).ToArray();
        }
        else
        {
            Roles = Roles.Append(@event.Role).ToArray();
        }
    }

    public void Handle(PostOrderedEvent @event)
    {
        UsedAdresses.Add(@event.Address);
    }

    public void Handle(StockAddedEvent @event)
    {
        PlantsCared++;
    }

    public void Handle(DeliveryConfirmedEvent @event)
    {
        PlantsSold++;
    }

    public void Handle(InstructionCreatedEvent @event)
    {
        InstructionCreated++;
    }

    private class PlantStockSubscription : IAggregateSubscription<User, PlantStock>
    {
        public IEnumerable<EventSubscriptionBase<User, PlantStock>> Subscriptions => new[]
        {
            new EventSubscription<User, PlantStock, StockAddedEvent>(new(
                @event => @event.CaretakerUsername.ToGuid(),
                (events, user) => events.Select(_=>user.TransposeSubscribedEvent(_))))
        };
    }

    private class PlantPostSubscription : IAggregateSubscription<User, PlantPost>
    {
        public IEnumerable<EventSubscriptionBase<User, PlantPost>> Subscriptions => new[]
        {
            new EventSubscription<User, PlantPost, PostOrderedEvent>(new(
                @event => @event.BuyerUsername.ToGuid(),
                (events, user) => events.Select(_=>user.TransposeSubscribedEvent(_))))
        };
    }

    private class PlantOrderSubscription : IAggregateSubscription<User, PlantOrder>
    {
        public IEnumerable<EventSubscriptionBase<User, PlantOrder>> Subscriptions => new[]
        {
            EventSubscriptionFactory.CreateForwarded<User, PlantOrder, DeliveryConfirmedEvent>(@event => @event.SellerUsername.ToGuid())
        };
    }

    private class PlantInstructionSubscription : IAggregateSubscription<User, PlantInstruction>
    {
        public IEnumerable<EventSubscriptionBase<User, PlantInstruction>> Subscriptions => new[]
        {
            EventSubscriptionFactory.CreateForwarded<User, PlantInstruction, InstructionCreatedEvent>(@event => @event.WriterUsername.ToGuid())
        };
    }
}
