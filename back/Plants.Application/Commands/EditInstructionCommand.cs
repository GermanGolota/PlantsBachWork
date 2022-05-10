using MediatR;

namespace Plants.Application.Commands
{
    public record EditInstructionCommand(int InstructionId, string Text,
        string Title, string Description, byte[]? CoverImage) : IRequest<EditInstructionResult>;
    public record EditInstructionResult();
}
