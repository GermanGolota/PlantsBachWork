using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantStocks;
using Plants.Presentation.Examples;
using Plants.Presentation.Extensions;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation.Controllers.v2;

[ApiController]
[Route("v2/plants")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class PlantsController : ControllerBase
{
    private readonly CommandMetadataFactory _metadataFactory;
    private readonly ICommandSender _sender;

    public PlantsController(CommandMetadataFactory metadataFactory, ICommandSender sender)
    {
        _metadataFactory = metadataFactory;
        _sender = sender;
    }

    [HttpPost("add")]
    [SwaggerRequestExample(typeof(PlantStockDto), typeof(AddPlantRequestExample))]
    [ApiVersion("2")]
    public async Task<IActionResult> Create([FromForm]AddPlantDto2 request, CancellationToken token)
    {
        var pictures = await Task.WhenAll(request.Files.Select(file => file.ReadBytesAsync()));
        var meta = _metadataFactory.Create<AddToStockCommand>(new(Guid.NewGuid(), nameof(PlantStock)));
        var command = new AddToStockCommand(meta, request.Plant, pictures);
        var result = await _sender.SendCommandAsync(command);
        return result.Match<IActionResult>(
            success => Ok(command.Metadata.Aggregate.Id),
            failure => BadRequest(failure.Reasons)
            );
    }

}

public record AddPlantDto2(PlantStockDto Plant, List<IFormFile> Files);
