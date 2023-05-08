namespace Plants.Aggregates;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Manager, Read)]
[Allow(Manager, Write)]
public class PlantStock : AggregateBase, 
    IEventHandler<StockAddedEvent>, IEventHandler<StockEdditedEvent>, 
    IDomainCommandHandler<PostStockItemCommand>, IDomainCommandHandler<AddToStockCommand>,
    IDomainCommandHandler<EditStockItemCommand>, IEventHandler<PostRemovedEvent>
{
    public PlantStock(Guid id) : base(id)
    {
    }

    public PlantInformation Information { get; private set; }
    public Picture[] Pictures { get; private set; }
    public User Caretaker { get; set; }
    public DateTime CreatedTime { get; private set; }
    public bool BeenPosted { get; private set; } = false;

    public void Handle(StockAddedEvent @event)
    {
        Information = @event.Plant;
        Pictures = @event.Pictures;
        CreatedTime = @event.CreatedTime;

        Metadata.Referenced.Add(new(@event.CaretakerUsername.ToGuid(), nameof(User)));
    }

    public CommandForbidden? ShouldForbid(PostStockItemCommand command, IUserIdentity user) =>
        user.HasAnyRoles(Producer, Manager)
            .And((BeenPosted is false).ToForbidden("Cannot post already posted stock"));

    public IEnumerable<Event> Handle(PostStockItemCommand command)
    {
        BeenPosted = true;
        return new[]
        {
            new StockItemPostedEvent(EventFactory.Shared.Create<StockItemPostedEvent>(command), command.Metadata.UserName, command.Price, Information.FamilyNames)
        };
    }

    public CommandForbidden? ShouldForbid(AddToStockCommand command, IUserIdentity userIdentity) =>
        userIdentity.HasRole(Producer).And(this.RequireNew);

    public IEnumerable<Event> Handle(AddToStockCommand command) =>
        new[]
        {
            new StockAddedEvent(EventFactory.Shared.Create<StockAddedEvent>(command), command.Plant, command.CreatedTime, command.Pictures, command.Metadata.UserName)
        };

    public CommandForbidden? ShouldForbid(EditStockItemCommand command, IUserIdentity user)
    {
        var validIdentity = user.HasRole(Manager).Or(user.HasRole(Producer).And(IsCaretaker(user)));
        var notPosted = (BeenPosted is false).ToForbidden("Cannot edit stock after it was posted");
        return validIdentity.And(notPosted);
    }

    public IEnumerable<Event> Handle(EditStockItemCommand command) =>
        new[]
        {
            new StockEdditedEvent(EventFactory.Shared.Create<StockEdditedEvent>(command), command.Plant, command.NewPictures, command.RemovedPictureIds)
        };

    public void Handle(StockEdditedEvent @event)
    {
        Information = @event.Plant;
        Pictures = Pictures
            .Where(_ => @event.RemovedPictureIds?.NotContains(_.Id) ?? true)
            .Union(@event.NewPictures)
            .ToArray();
    }

    private CommandForbidden? IsCaretaker(IUserIdentity user) =>
        (user.UserName == Caretaker.Login).ToForbidden("Cannot eddit somebody elses stock item");

    public void Handle(PostRemovedEvent @event)
    {
        BeenPosted = false;
    }

    private class PlantPostSubscription : IAggregateSubscription<PlantStock, PlantPost>
    {
        public IEnumerable<EventSubscriptionBase<PlantStock, PlantPost>> Subscriptions => new[]
        {
            new EventSubscription<PlantStock, PlantPost, PostRemovedEvent>(
                new(@event => @event.Metadata.Aggregate.Id,
                    (events, post) => events.Select(@event => post.TransposeSubscribedEvent(@event))))
        };
    }
}
