namespace Plants.Aggregates;

public record PlantStockParams(bool IsMine) : ISearchParams;