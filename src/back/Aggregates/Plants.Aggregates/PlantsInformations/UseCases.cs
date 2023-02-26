namespace Plants.Aggregates;

public record GetTotalStats : IRequest<IEnumerable<TotalStatsViewResult>>;
public record GetFinancialStats(DateTime? From, DateTime? To) : IRequest<IEnumerable<FinancialStatsViewResult>>;

public record GetUsedPlantSpecifications : IRequest<PlantSpecifications>;

public record TotalStatsViewResult(string GroupName, decimal Income, long Instructions, long Popularity);
public record FinancialStatsViewResult(decimal Income, string GroupName, long SoldCount, long PercentSold);


public record PlantSpecifications(HashSet<string> Groups, HashSet<string> Regions, HashSet<string> Soils);
