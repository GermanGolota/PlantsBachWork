using Plants.Aggregates.PlantInstructions;
using Plants.Aggregates.PlantOrders;
using Plants.Aggregates.PlantPosts;
using Plants.Aggregates.PlantStocks;

namespace Plants.Aggregates.PlantStats;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
public class PlantStat : AggregateBase,
    IEventHandler<StockAddedEvent>, IEventHandler<InstructionCreatedEvent>,
    IEventHandler<StockItemPostedEvent>, IEventHandler<DeliveryConfirmedEvent>
{
    public PlantStat(Guid id) : base(id)
    {
    }

    public string GroupName { get; private set; } = null;
    public long PlantsCount { get; private set; } = 0;
    public List<DateTime> PlantsCountUpdated { get; private set; } = new();
    public long InstructionsCount { get; private set; } = 0;
    public List<DateTime> InstructionsCountUpdated { get; private set; } = new();
    public long PostedCount { get; private set; } = 0;
    public List<DateTime> PostedCountUpdated { get; private set; } = new();
    public long SoldCount { get; private set; } = 0;
    public List<DateTime> SoldCountUpdated { get; private set; } = new();
    public decimal Income { get; private set; } = 0;
    public List<IncomeUpdate> IncomeUpdated { get; private set; } = new();

    public record IncomeUpdate(DateTime Time, decimal Value);

    public void Handle(StockAddedEvent @event)
    {
        GroupName = @event.Plant.GroupName;
        PlantsCount++;
        PlantsCountUpdated.Add(@event.Metadata.Time);
    }

    public void Handle(InstructionCreatedEvent @event)
    {
        GroupName = @event.Instruction.GroupName;
        InstructionsCount++;
        InstructionsCountUpdated.Add(@event.Metadata.Time);
    }

    public void Handle(StockItemPostedEvent @event)
    {
        GroupName = @event.GroupName;
        PostedCount++;
        PostedCountUpdated.Add(@event.Metadata.Time);
    }

    public void Handle(DeliveryConfirmedEvent @event)
    {
        GroupName = @event.GroupName;
        SoldCount++;
        Income += @event.Price;
        IncomeUpdated.Add(new(@event.Metadata.Time, @event.Price));
    }

    private class PlantStockSubscription : IAggregateSubscription<PlantStat, PlantStock>
    {
        public IEnumerable<EventSubscriptionBase<PlantStat, PlantStock>> Subscriptions => new[]
        {
            new EventSubscription<PlantStat, PlantStock, StockAddedEvent>(
                new AggregateLoadingTranspose<PlantStat, StockAddedEvent>(
                    @event => @event.Plant.GroupName.ToGuid(),
                    (events, stats) =>
                        events.Select(@event => stats.TransposeSubscribedEvent(@event)))
                )
        };

    }

    private class PlantInstructionSubscription : IAggregateSubscription<PlantStat, PlantInstruction>
    {
        public IEnumerable<EventSubscriptionBase<PlantStat, PlantInstruction>> Subscriptions => new[]
        {
            EventSubscriptionFactory.CreateForwarded<PlantStat, PlantInstruction, InstructionCreatedEvent>(@event => @event.Instruction.GroupName.ToGuid())
        };
    }

    private class PlantPostSubscription : IAggregateSubscription<PlantStat, PlantPost>
    {
        public IEnumerable<EventSubscriptionBase<PlantStat, PlantPost>> Subscriptions => new[]
        {
            EventSubscriptionFactory.CreateForwarded<PlantStat, PlantPost, StockItemPostedEvent>(@event => @event.GroupName.ToGuid())
        };
    }

    private class PlantOrderSubscription : IAggregateSubscription<PlantStat, PlantOrder>
    {
        public IEnumerable<EventSubscriptionBase<PlantStat, PlantOrder>> Subscriptions => new[]
        {
            EventSubscriptionFactory.CreateForwarded<PlantStat, PlantOrder, DeliveryConfirmedEvent>(@event => @event.GroupName.ToGuid())
        };
    }
}