namespace Plants.Aggregates.PlantInstructions;

public record CreateInstructionCommand(CommandMetadata Metadata, InstructionModel Instruction, byte[] CoverImage) : Command(Metadata);
public record InstructionCreatedEvent(EventMetadata Metadata, InstructionModel Instruction, string CoverUrl, string WriterUsername, Guid InstructionId) : Event(Metadata);

public record EditInstructionCommand(CommandMetadata Metadata, InstructionModel Instruction, byte[] CoverImage) : Command(Metadata);
public record InstructionEditedEvent(EventMetadata Metadata, InstructionModel Instruction, string CoverUrl) : Event(Metadata);

public record InstructionModel(
    string GroupName, string Text, string Title,
    string Description);