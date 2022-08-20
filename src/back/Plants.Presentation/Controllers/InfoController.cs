using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Presentation.Controllers
{
    [ApiController]
    [Route("info")]
    public class InfoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InfoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("dicts")]
        public async Task<ActionResult<DictsResult>> Dicts(CancellationToken token)
        {
            var req = new DictsRequest();
            var res = await _mediator.Send(req, token);
            return Ok(res);
        }

        [HttpGet("addresses")]
        public async Task<ActionResult<AddressResult>> Addresses(CancellationToken token)
        {
            var req = new AddressRequest();
            var res = await _mediator.Send(req, token);
            return Ok(res);
        }
    }
}
