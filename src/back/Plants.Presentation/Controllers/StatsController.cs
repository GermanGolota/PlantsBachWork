using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("stats")]
public class StatsController : ControllerBase
{
    private readonly IMediator _query;

    public StatsController(IMediator query)
    {
        _query = query;
    }

    [HttpGet("financial")]
    public async Task<ActionResult<ListViewResult<FinancialStatsViewResult>>> Financial([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken token)
    {
        var stats = await _query.Send(new GetFinancialStats(from, to), token);
        return new ListViewResult<FinancialStatsViewResult>(stats);
    }

    [HttpGet("total")]
    public async Task<ActionResult<ListViewResult<TotalStatsViewResult>>> Total(CancellationToken token)
    {
        var stats = await _query.Send(new GetTotalStats(), token);
        return new ListViewResult<TotalStatsViewResult>(stats);
    }
}