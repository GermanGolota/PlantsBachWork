using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Requests;
using System.Threading.Tasks;

namespace Plants.Presentation.Controllers
{
    [ApiController]
    [Route("instructions")]
    public class InstructionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InstructionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("find")]
        public async Task<ActionResult<FindInstructionsResult>> Find([FromQuery] FindInstructionsRequest request)
        {
            return await _mediator.Send(request);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetInstructionResult>> Get([FromRoute] int id)
        {
            return await _mediator.Send(new GetInstructionRequest(id));
        }
    }
}
