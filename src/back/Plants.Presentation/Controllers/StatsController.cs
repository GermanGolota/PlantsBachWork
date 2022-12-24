using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Requests;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("stats")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class StatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("financial")]
    public async Task<ActionResult<FinancialStatsResult>> Financial([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken token)
    {
        var req = new FinancialStatsRequest(from, to);
        var res = await _mediator.Send(req, token);
        return Ok(res);
    }

    [HttpGet("total")]
    public async Task<ActionResult<TotalStatsResult>> Total(CancellationToken token)
    {
        var req = new TotalStatsRequest();
        var res = await _mediator.Send(req, token);
        return Ok(res);
    }
}
