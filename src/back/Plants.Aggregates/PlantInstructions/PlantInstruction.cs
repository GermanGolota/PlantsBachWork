namespace Plants.Aggregates.PlantInstructions;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
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

public record CreateInstructionCommand(CommandMetadata Metadata, InstructionModel Instruction, byte[] CoverImage) : Command(Metadata);
public record InstructionCreatedEvent(EventMetadata Metadata, InstructionModel Instruction, string CoverUrl, string WriterUsername, Guid InstructionId) : Event(Metadata);

public record EditInstructionCommand(CommandMetadata Metadata, InstructionModel Instruction, byte[] CoverImage) : Command(Metadata);
public record InstructionEditedEvent(EventMetadata Metadata, InstructionModel Instruction, string CoverUrl) : Event(Metadata);

public record InstructionModel(
    string GroupName, string Text, string Title,
    string Description);