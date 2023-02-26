namespace Plants.Aggregates;

// Commands

public record CreateInstructionCommand(CommandMetadata Metadata, InstructionModel Instruction, byte[] CoverImage) : Command(Metadata);
public record InstructionCreatedEvent(EventMetadata Metadata, InstructionModel Instruction, string CoverUrl, string WriterUsername, Guid InstructionId) : Event(Metadata);

public record EditInstructionCommand(CommandMetadata Metadata, InstructionModel Instruction, byte[] CoverImage) : Command(Metadata);
public record InstructionEditedEvent(EventMetadata Metadata, InstructionModel Instruction, string CoverUrl) : Event(Metadata);

// Queries

public record SearchInstructions(PlantInstructionParams Parameters, QueryOptions Options) : IRequest<IEnumerable<FindInstructionsViewResultItem>>;
public record GetInstruction(Guid InstructionId) : IRequest<GetInstructionViewResultItem?>;

public record FindInstructionsViewResultItem(Guid Id, string Title, string Description, string CoverUrl);
public record PlantInstructionParams(string GroupName, string Title, string Description) : ISearchParams;

// Types

public record GetInstructionViewResultItem(Guid Id, string Title, string Description,
    string InstructionText, string CoverUrl, string PlantGroupName);

public record InstructionModel(
    string GroupName, string Text, string Title,
    string Description);
