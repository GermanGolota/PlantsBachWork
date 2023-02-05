using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("stats")]
public class StatsController : ControllerBase
{
    private readonly IProjectionQueryService<PlantTotalStat> _statQuery;
    private readonly IProjectionQueryService<PlantTimedStat> _timedStatQuery;

    public StatsController(IProjectionQueryService<PlantTotalStat> totalStatQuery,
        IProjectionQueryService<PlantTimedStat> timedStatQuery)
    {
        _statQuery = totalStatQuery;
        _timedStatQuery = timedStatQuery;
    }

    public record FinancialStatsViewResult(decimal Income, string GroupName, long SoldCount, long PercentSold);

    [HttpGet("financial")]
    public async Task<ActionResult<ListViewResult<FinancialStatsViewResult>>> Financial([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken token)
    {
        var stats = await _timedStatQuery.FindAllAsync(_ => true, token);
        List<FinancialStatsViewResult> results = new();
        return new ListViewResult<FinancialStatsViewResult>(stats.Where(_ => IsInRange(_.Date, from, to)).GroupBy(stat => stat.GroupName)
            .Select(pair =>
            {
                var sold = pair.Sum(_ => _.SoldCount);
                var plants = pair.Sum(_ => _.PlantsCount);
                return new FinancialStatsViewResult(pair.Sum(_ => _.Income), pair.Key, sold, plants is 0 ? 0 : sold / plants);
            })
            .ToList());
    }

    private bool IsInRange(DateTime time, DateTime? from, DateTime? to) =>
        (from is null || time > from) && (to is null || time < to);

    public record TotalStatsViewResult(string GroupName, decimal Income, long Instructions, long Popularity);

    [HttpGet("total")]
    public async Task<ActionResult<ListViewResult<TotalStatsViewResult>>> Total(CancellationToken token)
    {
        var stats = (await _statQuery.FindAllAsync(_ => true, token)).ToList();
        return new ListViewResult<TotalStatsViewResult>(stats.Select(stat => new TotalStatsViewResult(stat.GroupName, stat.Income, stat.InstructionsCount, stat.PlantsCount)));
    }
}