using MediatR;
using Microsoft.AspNetCore.Mvc;
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
    }
}
