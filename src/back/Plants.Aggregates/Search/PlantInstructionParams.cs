namespace Plants.Aggregates.Search;

public record PlantInstructionParams(string GroupName, string Title, string Description) : ISearchParams;