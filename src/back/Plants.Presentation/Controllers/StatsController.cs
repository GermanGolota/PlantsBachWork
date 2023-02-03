using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("stats")]
public class StatsController : ControllerBase
{
    private readonly IProjectionQueryService<PlantTotalStat> _statQuery;
    private readonly IProjectionQueryService<PlantTimedStat> _timedStatQuery;
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;

    public StatsController(IProjectionQueryService<PlantTotalStat> totalStatQuery,
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
        List<GroupFinancialStats2> results = new();
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