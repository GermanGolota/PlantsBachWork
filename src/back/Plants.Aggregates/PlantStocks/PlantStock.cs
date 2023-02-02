using Plants.Aggregates.Users;
using Plants.Domain.Abstractions;
using Plants.Domain.Aggregate;

namespace Plants.Aggregates.PlantStocks;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Manager, Read)]
[Allow(Manager, Write)]
public class PlantStock : AggregateBase, IEventHandler<StockAddedEvent>, IEventHandler<StockEdditedEvent>, IDomainCommandHandler<PostStockItemCommand>
{
    public PlantStock(Guid id) : base(id)
    {
    }

    public PlantInformation Information { get; private set; }
    public string[] PictureUrls { get; private set; }
    public User Caretaker { get; set; }
    public DateTime CreatedTime { get; private set; }
    public bool BeenPosted { get; private set; } = false;

    public void Handle(StockAddedEvent @event)
    {
        Information = @event.Plant;
        PictureUrls = @event.PictureUrls;
        Metadata.Referenced.Add(new(@event.CaretakerUsername.ToGuid(), nameof(User)));
        CreatedTime = @event.CreatedTime;
    }

    public void Handle(StockEdditedEvent @event)
    {
        Information = @event.Plant;
        PictureUrls = PictureUrls.Except(@event.RemovedPictureUrls).Union(@event.NewPictureUrls).ToArray();
    }

    public CommandForbidden? ShouldForbid(PostStockItemCommand command, IUserIdentity user) =>
        user.HasAnyRoles(Producer, Manager)
            .And((BeenPosted is false).ToForbidden("Cannot post already posted stock"));

    public IEnumerable<Event> Handle(PostStockItemCommand command)
    {
        BeenPosted = true;
        return new[]
        {
            new StockItemPostedEvent(EventFactory.Shared.Create<StockItemPostedEvent>(command), command.Metadata.UserName, command.Price, Information.GroupName)
        };
    }

}