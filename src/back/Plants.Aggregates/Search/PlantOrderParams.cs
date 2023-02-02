namespace Plants.Aggregates;

public record PlantOrderParams(bool OnlyMine) : ISearchParams;