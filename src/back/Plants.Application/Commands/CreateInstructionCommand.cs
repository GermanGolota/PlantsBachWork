using MediatR;

namespace Plants.Application.Commands;

public record CreateInstructionCommand(long GroupId, string Text,
    string Title, string Description, byte[]? CoverImage) : IRequest<CreateInstructionResult>;
public record CreateInstructionResult(long Id);
