namespace Plants.Aggregates;

// Queries

public record GetTotalStats : IRequest<IEnumerable<TotalStatsViewResult>>;
public record GetFinancialStats(DateTime? From, DateTime? To) : IRequest<IEnumerable<FinancialStatsViewResult>>;
public record GetUsedPlantSpecifications : IRequest<PlantSpecifications>;

// Types

public record TotalStatsViewResult(string FamilyName, decimal Income, long Instructions, long Popularity);
public record FinancialStatsViewResult(decimal Income, string FamilyName, long SoldCount, long PercentSold);
public record PlantSpecifications(HashSet<string> Families, HashSet<string> Regions, HashSet<string> Soils);
