using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Commands;
using Plants.Application.Requests;
using System.Threading.Tasks;

namespace Plants.Presentation.Controllers
{
    [ApiController]
    [Route("post")]
    public class PostController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PostController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetPost([FromRoute] int id)
        {
            return Ok(await _mediator.Send(new PostRequest(id)));
        }

        [HttpPost("{id}")]
        public async Task<ActionResult> Order([FromRoute] int id, [FromQuery] string city, [FromQuery] int mailNumber)
        {
            return Ok(await _mediator.Send(new PlaceOrderCommand(id, city, mailNumber)));
        }
    }
}
