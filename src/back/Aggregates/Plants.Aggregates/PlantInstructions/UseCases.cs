namespace Plants.Aggregates;

// Commands

public record CreateInstructionCommand(CommandMetadata Metadata, InstructionModel Instruction, Picture CoverImage) : Command(Metadata);
public record InstructionCreatedEvent(EventMetadata Metadata, InstructionModel Instruction, Picture CoverImage, string WriterUsername, Guid InstructionId) : Event(Metadata);

public record EditInstructionCommand(CommandMetadata Metadata, InstructionModel Instruction, Picture CoverImage) : Command(Metadata);
public record InstructionEditedEvent(EventMetadata Metadata, InstructionModel Instruction, Picture CoverImage) : Event(Metadata);

// Queries

public record SearchInstructions(PlantInstructionParams Parameters, QueryOptions Options) : IRequest<IEnumerable<FindInstructionsViewResultItem>>;
public record GetInstruction(Guid InstructionId) : IRequest<GetInstructionViewResultItem?>;

// Types

public record GetInstructionViewResultItem(Guid Id, string Title, string Description,
    string InstructionText, string CoverUrl, string PlantFamilyName);

public record InstructionModel(
    string FamilyName, string Text, string Title,
    string Description);

public record FindInstructionsViewResultItem(Guid Id, string Title, string Description, string CoverUrl);
public record PlantInstructionParams(string FamilyName, string? Title, string? Description) : ISearchParams;
