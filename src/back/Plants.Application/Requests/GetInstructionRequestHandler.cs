using MediatR;
using Plants.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Requests
{
    public class GetInstructionRequestHandler : IRequestHandler<GetInstructionRequest, GetInstructionResult>
    {
        private readonly IInstructionsService _instructions;

        public GetInstructionRequestHandler(IInstructionsService instructions)
        {
            _instructions = instructions;
        }

        public async Task<GetInstructionResult> Handle(GetInstructionRequest request, CancellationToken cancellationToken)
        {
            var res = await _instructions.GetBy(request.Id);
            GetInstructionResult output;
            if (res is not null)
            {
                output = new GetInstructionResult(true, res);
            }
            else
            {
                output = new GetInstructionResult(false, null);
            }
            return output;
        }
    }
}
