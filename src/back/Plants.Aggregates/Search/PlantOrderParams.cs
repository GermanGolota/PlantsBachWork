namespace Plants.Aggregates.Search;

public record PlantOrderParams(bool OnlyMine) : ISearchParams;