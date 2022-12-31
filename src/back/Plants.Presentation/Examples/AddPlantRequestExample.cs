using Plants.Aggregates.PlantStocks;
using Plants.Presentation.Controllers.v2;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation.Examples;

internal class AddPlantRequestExample : IMultipleExamplesProvider<PlantInformation>
{
    public IEnumerable<SwaggerExample<PlantInformation>> GetExamples()
    {
        yield return new()
        {
            Name = "Nanjing apple",
            Value = new(
            "Nanjing Apple",
            "Apple from Nanjing",
            new[] { "Taiga", "Forest" },
            "Sandy",
            "Apple",
            new DateTime(2022, 12, 12))
        };
    }
}