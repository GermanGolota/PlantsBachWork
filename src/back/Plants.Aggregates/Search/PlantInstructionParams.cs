namespace Plants.Aggregates;

public record PlantInstructionParams(/*string GroupName, */string Title, string Description) : ISearchParams;