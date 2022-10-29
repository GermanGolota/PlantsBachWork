using MediatR;

namespace Plants.Application.Commands;

public record CreateInstructionCommand(int GroupId, string Text,
    string Title, string Description, byte[]? CoverImage) : IRequest<CreateInstructionResult>;
public record CreateInstructionResult(int Id);
