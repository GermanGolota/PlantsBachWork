using Plants.Presentation.Controllers;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation.Examples;

internal class AddPlantRequestExample : IExamplesProvider<AddPlantDto>
{
    public AddPlantDto GetExamples() =>
        new AddPlantDto(
            "Example plant name",
            "Example plant description",
            new[] { 1, 2 },
            1,
            1,
            new DateTime(2022, 12, 12),
            new byte[][] { }
            );
}