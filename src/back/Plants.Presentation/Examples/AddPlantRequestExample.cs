using Plants.Presentation.Controllers.v2;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation.Examples;

internal class AddPlantRequestExample : IExamplesProvider<AddPlantDto2>
{
    public AddPlantDto2 GetExamples() =>
        new AddPlantDto2(
            "Nanjing Apple",
            "Apple from Nanjing",
            new[] { "Taiga", "Forest" },
            "Sandy",
            "Apple",
            new DateTime(2022, 12, 12),
            new byte[][] { }
            );
}