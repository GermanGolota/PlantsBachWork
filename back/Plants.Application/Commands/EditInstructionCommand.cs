using MediatR;

namespace Plants.Application.Commands
{
    public record EditInstructionCommand(int InstructionId,  int GroupId, string Text,
        string Title, string Description, byte[]? CoverImage) : IRequest<EditInstructionResult>;
    public record EditInstructionResult();
}
