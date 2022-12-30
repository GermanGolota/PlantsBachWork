namespace Plants.Aggregates.Search;

public record PlantStockParams(bool IsMine) : ISearchParams;