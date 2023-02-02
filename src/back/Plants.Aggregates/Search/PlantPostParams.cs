namespace Plants.Aggregates;

public record PlantPostParams(
    string PlantName,
    decimal? LowerPrice,
    decimal? TopPrice,
    DateTime? LastDate/*,
    string[] Groups,
    string[] Regions,
    string[] Soils*/) : ISearchParams;
