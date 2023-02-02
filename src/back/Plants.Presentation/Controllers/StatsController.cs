using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Requests;

namespace Plants.Presentation;

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
    private readonly IProjectionQueryService<PlantTotalStat> _statQuery;
    private readonly IProjectionQueryService<PlantTimedStat> _timedStatQuery;
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;

    public StatsControllerV2(IProjectionQueryService<PlantTotalStat> totalStatQuery,
        IProjectionQueryService<PlantTimedStat> timedStatQuery,
        IProjectionQueryService<PlantInfo> infoQuery)
    {
        _statQuery = totalStatQuery;
        _timedStatQuery = timedStatQuery;
        _infoQuery = infoQuery;
    }

    [HttpGet("financial")]
    public async Task<ActionResult<FinancialStatsResult2>> Financial([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken token)
    {
        var stats = await _timedStatQuery.FindAllAsync(_ => true, token);
        var groups = (await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token)).GroupNames.ToInverse();
        List<GroupFinancialStats> results = new();
        return new FinancialStatsResult2(stats.Where(_ => IsInRange(_.Date, from, to)).GroupBy(stat => stat.GroupName)
            .Select(pair =>
            {
                var sold = pair.Sum(_ => _.SoldCount);
                var plants = pair.Sum(_ => _.PlantsCount);
                return new GroupFinancialStats2
                {
                    GroupId = groups[pair.Key].ToString(),
                    GroupName = pair.Key,
                    Income = pair.Sum(_ => _.Income),
                    SoldCount = sold,
                    PercentSold = plants is 0 ? 0 : sold / plants
                };
            })
            .ToList());
    }

    private bool IsInRange(DateTime time, DateTime? from, DateTime? to) =>
        (from is null || time > from) && (to is null || time < to);

    [HttpGet("total")]
    public async Task<ActionResult<TotalStatsResult2>> Total(CancellationToken token)
    {
        var stats = await _statQuery.FindAllAsync(_ => true, token);
        var groups = (await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token)).GroupNames.ToInverse();
        return new TotalStatsResult2(stats.Select(stat => new GroupTotalStats2(groups[stat.GroupName].ToString(), stat.GroupName, stat.Income, stat.InstructionsCount, stat.PlantsCount)));
    }
}