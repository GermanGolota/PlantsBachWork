using Plants.Aggregates.PlantInstructions;
using Plants.Aggregates.PlantOrders;
using Plants.Aggregates.PlantStocks;
using Plants.Domain.Aggregate;

namespace Plants.Aggregates.PlantStats;

public abstract class PlantStat : AggregateBase,
    IEventHandler<StockAddedEvent>, IEventHandler<InstructionCreatedEvent>,
    IEventHandler<StockItemPostedEvent>, IEventHandler<DeliveryConfirmedEvent>
{
    protected PlantStat(Guid id) : base(id)
    {
    }

    public long PlantsCount { get; private set; } = 0;
    public long InstructionsCount { get; private set; } = 0;
    public long PostedCount { get; private set; } = 0;
    public long SoldCount { get; private set; } = 0;
    public decimal Income { get; private set; } = 0;

    public void Handle(StockAddedEvent @event)
    {
        PlantsCount++;
    }

    public void Handle(InstructionCreatedEvent @event)
    {
        InstructionsCount++;
    }

    public void Handle(StockItemPostedEvent @event)
    {
        PostedCount++;
    }

    public void Handle(DeliveryConfirmedEvent @event)
    {
        SoldCount++;
        Income += @event.Price;
    }

}
