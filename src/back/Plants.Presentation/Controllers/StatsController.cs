using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantInfos;
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
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;

    public StatsControllerV2(IProjectionQueryService<PlantStat> statQuery, IProjectionQueryService<PlantInfo> infoQuery)
    {
        _statQuery = statQuery;
        _infoQuery = infoQuery;
    }

    [HttpGet("financial")]
    public async Task<ActionResult<FinancialStatsResult>> Financial([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken token)
    {
        var stats = await _statQuery.FindAllAsync(_ => true);
        var groups = (await _infoQuery.GetByIdAsync(PlantInfo.InfoId)).GroupNames.ToInverse();
        List<GroupFinancialStats> results = new();
        foreach (var stat in stats)
        {
            results.Add(new()
            {
                GroupId = groups[stat.GroupName],
                GroupName = stat.GroupName,
                Income = stat.IncomeUpdated.Where(_ => IsInRange(_.Time, from, to)).Sum(_ => _.Value),
                SoldCount = stat.SoldCountUpdated.Where(_ => IsInRange(_, from, to)).Count(),
                PercentSold = stat.SoldCountUpdated.Where(_ => IsInRange(_, from, to)).Count() / stat.PostedCountUpdated.Where(_ => _ > from && _ < to).Count()
            });
        }
        return new FinancialStatsResult(results);
    }

    private bool IsInRange(DateTime time, DateTime? from, DateTime? to) =>
        (from is null || time > from) && (to is null || time < to);

    [HttpGet("total")]
    public async Task<ActionResult<TotalStatsResult>> Total(CancellationToken token)
    {
        var stats = await _statQuery.FindAllAsync(_ => true);
        var groups = (await _infoQuery.GetByIdAsync(PlantInfo.InfoId)).GroupNames.ToInverse();
        return new TotalStatsResult(stats.Select(stat => new GroupTotalStats(groups[stat.GroupName], stat.GroupName, stat.Income, stat.InstructionsCount, stat.PlantsCount)));
    }
}