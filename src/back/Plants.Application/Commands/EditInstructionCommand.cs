using MediatR;

namespace Plants.Application.Commands;

public record EditInstructionCommand(long InstructionId, long GroupId, string Text,
    string Title, string Description, byte[]? CoverImage) : IRequest<EditInstructionResult>;
public record EditInstructionResult(long InstructionId);

public record EditInstructionResult2(Guid InstructionId);
