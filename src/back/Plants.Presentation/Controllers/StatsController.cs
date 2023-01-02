using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantStats;
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


[ApiController]
[Route("v2/stats")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class StatsControllerV2 : ControllerBase
{
    private readonly IProjectionQueryService<PlantStat> _statQuery;

    public StatsControllerV2(IProjectionQueryService<PlantStat> statQuery)
    {
        _statQuery = statQuery;
    }

    [HttpGet("financial")]
    public async Task<ActionResult<FinancialStatsResult>> Financial([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken token)
    {

        throw new NotImplementedException();
    }

    [HttpGet("total")]
    public async Task<ActionResult<TotalStatsResult>> Total(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}