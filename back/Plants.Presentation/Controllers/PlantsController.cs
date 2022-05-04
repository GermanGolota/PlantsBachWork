using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Commands;
using Plants.Application.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Presentation.Controllers
{
    [ApiController]
    [Route("plants")]
    public class PlantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("notposted")]
        public async Task<ActionResult<PlantsResult>> GetNotPosted(CancellationToken token)
        {
            return await _mediator.Send(new PlantsRequest(), token);
        }

        [HttpGet("prepared/{id}")]
        public async Task<ActionResult<PreparedPostResult>> GetPrepared(int id, CancellationToken token)
        {
            return await _mediator.Send(new PreparedPostRequest(id), token);
        }

        [HttpPost("{id}/post")]
        public async Task<ActionResult<CreatePostResult>> GetPrepared([FromRoute] int id,
            [FromQuery] decimal price, CancellationToken token)
        {
            return await _mediator.Send(new CreatePostCommand(id, price), token);
        }
    }
}
