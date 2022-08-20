using MediatR;
using Plants.Application.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Requests
{
    public class FindInstructionsRequestHandler : IRequestHandler<FindInstructionsRequest, FindInstructionsResult>
    {
        private readonly IInstructionsService _instructions;

        public FindInstructionsRequestHandler(IInstructionsService instructions)
        {
            _instructions = instructions;
        }

        public async Task<FindInstructionsResult> Handle(FindInstructionsRequest request,
            CancellationToken cancellationToken)
        {
            var items = await _instructions.GetFor(request.GroupId, request.Title, request.Description);
            return new FindInstructionsResult(items.ToList());
        }
    }
}
