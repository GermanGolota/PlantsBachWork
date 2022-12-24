using Plants.Presentation.Controllers.v2;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation.Examples;

internal class AddPlantRequestExample : IExamplesProvider<AddPlantDto2>
{
    public AddPlantDto2 GetExamples() =>
        new AddPlantDto2(
            "Example plant name",
            "Example plant description",
            new[] { 1, 2 },
            1,
            1,
            new DateTime(2022, 12, 12),
            new byte[][] { }
            );
}