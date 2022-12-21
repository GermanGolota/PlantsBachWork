namespace Plants.Aggregates.PlantStocks;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
public class PlantStock : AggregateBase, IDomainCommandHandler<AddToStockCommand>, IEventHandler<StockAddedEvent>
{
    public PlantStock(Guid id) : base(id)
    {
    }

    public string PlantName { get; private set; }

    public CommandForbidden? ShouldForbid(AddToStockCommand command, IUserIdentity userIdentity) =>
         userIdentity.HasRole(Producer)
        .And(this.RequireNew);

    public IEnumerable<Event> Handle(AddToStockCommand command) =>
         new[]
        {
            new StockAddedEvent(EventFactory.Shared.Create<StockAddedEvent>(command, Version), command.Plant)
        };

    public void Handle(StockAddedEvent @event)
    {
        PlantName = @event.Plant.PlantName;
    }
}

public record AddToStockCommand(CommandMetadata Metadata, PlantStockDto Plant) : Command(Metadata);
public record StockAddedEvent(EventMetadata Metadata, PlantStockDto Plant) : Event(Metadata);
public record PlantStockDto(string PlantName);