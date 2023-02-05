using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("stats")]
public class StatsController : ControllerBase
{
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;

    public StatsController(IProjectionQueryService<PlantInfo> infoQuery)
    {
        _infoQuery = infoQuery;
    }

    public record FinancialStatsViewResult(decimal Income, string GroupName, long SoldCount, long PercentSold);

    [HttpGet("financial")]
    public async Task<ActionResult<ListViewResult<FinancialStatsViewResult>>> Financial([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken token)
    {
        var info = await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token);

        return new ListViewResult<FinancialStatsViewResult>(info.FinancialStats
            .Where(x => IsInRange(DateTime.Parse(x.Key), from, to))
            .SelectMany(_ => _.Value)
            .GroupBy(stat => stat.Key)
            .Select(pair =>
            {
                var sold = pair.Sum(_ => _.Value.SoldCount);
                var plants = pair.Sum(_ => _.Value.PlantsCount);
                return new FinancialStatsViewResult(pair.Sum(_ => _.Value.Income), pair.Key, sold, plants is 0 ? 0 : sold / plants);
            }));
    }

    private bool IsInRange(DateTime time, DateTime? from, DateTime? to) =>
        (from is null || time > from) && (to is null || time < to);

    public record TotalStatsViewResult(string GroupName, decimal Income, long Instructions, long Popularity);

    [HttpGet("total")]
    public async Task<ActionResult<ListViewResult<TotalStatsViewResult>>> Total(CancellationToken token)
    {
        var info = await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token);
        return new ListViewResult<TotalStatsViewResult>(info.TotalStats
            .Select(stat => new TotalStatsViewResult(stat.Key, stat.Value.Income, stat.Value.InstructionsCount, stat.Value.PlantsCount))
            );
    }
}