using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Requests;
using System.Threading.Tasks;

namespace Plants.Presentation.Controllers
{
    [ApiController]
    [Route("order")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("/{id}")]
        public async Task<ActionResult> GetOrder([FromRoute] int id)
        {
            return Ok(await _mediator.Send(new OrderRequest(id)));
        }
    }
}
