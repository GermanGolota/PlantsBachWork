namespace Plants.Aggregates.PlantInstructions;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Manager, Read)]
[Allow(Manager, Write)]
public class PlantInstruction : AggregateBase,
    IEventHandler<InstructionCreatedEvent>, IEventHandler<InstructionEditedEvent>
{
    public PlantInstruction(Guid id) : base(id)
    {
    }

    public InstructionModel Information { get; private set; }
    public string CoverUrl { get; private set; }

    public void Handle(InstructionCreatedEvent @event)
    {
        Information = @event.Instruction;
        CoverUrl = @event.CoverUrl;
    }

    public void Handle(InstructionEditedEvent @event)
    {
        Information = @event.Instruction;
        if (@event.CoverUrl is null)
        {
            CoverUrl = @event.CoverUrl;
        }
    }

}
