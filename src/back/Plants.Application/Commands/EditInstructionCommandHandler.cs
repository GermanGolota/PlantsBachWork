using MediatR;
using Plants.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Commands
{
    public class EditInstructionCommandHandler : IRequestHandler<EditInstructionCommand, EditInstructionResult>
    {
        private readonly IInstructionsService _instructions;

        public EditInstructionCommandHandler(IInstructionsService instructions)
        {
            _instructions = instructions;
        }

        public async Task<EditInstructionResult> Handle(EditInstructionCommand request, CancellationToken cancellationToken)
        {
            await _instructions.Edit(request.InstructionId, request.GroupId,
                request.Text, request.Title, request.Description, request.CoverImage);
            return new EditInstructionResult(request.InstructionId);
        }
    }
}
