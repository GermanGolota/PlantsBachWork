using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Requests;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("info")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
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
