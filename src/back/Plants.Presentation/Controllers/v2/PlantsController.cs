using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantStocks;
using Plants.Domain;
using Plants.Domain.Services;
using Plants.Presentation.Examples;
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
    [SwaggerRequestExample(typeof(AddPlantDto), typeof(AddPlantRequestExample))]
    [ApiVersion("2")]
    public async Task<IActionResult> Create
        ([FromBody] AddPlantDto2 body, CancellationToken token)
    {
        var meta = _metadataFactory.Create<AddToStockCommand>(new(Guid.NewGuid(), nameof(PlantStock)));
        var command = new AddToStockCommand(meta, new PlantStockDto(body.Name, body.Description, body.Regions, body.SoilName, body.GroupName, body.Created, body.Pictures));
        var result = await _sender.SendCommandAsync(command);
        return result.Match<IActionResult>(success => Ok(command.Metadata.Aggregate.Id), failure => BadRequest(failure.Reasons));
    }

}

public record AddPlantDto2(string Name, string Description, string[] Regions, string SoilName, string GroupName, DateTime Created, byte[][] Pictures);
