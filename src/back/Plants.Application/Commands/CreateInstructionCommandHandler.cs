using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Commands;

public class CreateInstructionCommandHandler : IRequestHandler<CreateInstructionCommand, CreateInstructionResult>
{
    private readonly IInstructionsService _instructions;

    public CreateInstructionCommandHandler(IInstructionsService instructions)
    {
        _instructions = instructions;
    }

    public async Task<CreateInstructionResult> Handle(CreateInstructionCommand request, CancellationToken cancellationToken)
    {
        var id = await _instructions.Create(request.GroupId, request.Text, 
            request.Title, request.Description, request.CoverImage);
        return new CreateInstructionResult(id);
    }
}
