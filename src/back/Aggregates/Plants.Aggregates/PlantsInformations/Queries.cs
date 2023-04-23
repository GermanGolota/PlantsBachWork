using Humanizer;

namespace Plants.Aggregates;

internal sealed class GetTotalStatsHandler : IRequestHandler<GetTotalStats, IEnumerable<TotalStatsViewResult>>
{
    private readonly IProjectionQueryService<PlantsInformation> _query;

    public GetTotalStatsHandler(IProjectionQueryService<PlantsInformation> query)
    {
        _query = query;
    }

    public async Task<IEnumerable<TotalStatsViewResult>> Handle(GetTotalStats request, CancellationToken token)
    {
        var info = await _query.GetByIdAsync(PlantsInformation.InfoId, token);
        return info.TotalStats.Select(stat => new TotalStatsViewResult(stat.Key, stat.Value.Income, stat.Value.InstructionsCount, stat.Value.PlantsCount));
    }

}

internal sealed class GetFinancialStatsHandler : IRequestHandler<GetFinancialStats, IEnumerable<FinancialStatsViewResult>>
{
    private readonly IProjectionQueryService<PlantsInformation> _query;

    public GetFinancialStatsHandler(IProjectionQueryService<PlantsInformation> query)
    {
        _query = query;
    }

    public async Task<IEnumerable<FinancialStatsViewResult>> Handle(GetFinancialStats request, CancellationToken token)
    {
        var info = await _query.GetByIdAsync(PlantsInformation.InfoId, token);

        return info.DailyStats
            .Where(x => IsInRange(DateTime.Parse(x.Key), request.From, request.To))
            .SelectMany(_ => _.Value)
            .GroupBy(stat => stat.Key)
            .Select(pair =>
            {
                var sold = pair.Sum(_ => _.Value.SoldCount);
                var plants = pair.Sum(_ => _.Value.PlantsCount);
                return new FinancialStatsViewResult(pair.Sum(_ => _.Value.Income), pair.Key, sold, plants is 0 ? 0 : sold / plants);
            });
    }

    private bool IsInRange(DateTime time, DateTime? from, DateTime? to) =>
        (from is null || time > from) && (to is null || time < to);

}

internal sealed record GetUsedPlantSpecificationsHandler : IRequestHandler<GetUsedPlantSpecifications, PlantSpecifications>
{
    private readonly IProjectionQueryService<PlantsInformation> _query;

    public GetUsedPlantSpecificationsHandler(IProjectionQueryService<PlantsInformation> query)
    {
        _query = query;
    }

    public async Task<PlantSpecifications> Handle(GetUsedPlantSpecifications request, CancellationToken cancellationToken)
    {
        var dicts = await _query.GetByIdAsync(PlantsInformation.InfoId, cancellationToken);
        return new PlantSpecifications(dicts.FamilyNames, dicts.RegionNames, dicts.SoilNames);
    }
}
