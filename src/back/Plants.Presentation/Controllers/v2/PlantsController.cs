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
    public async Task<ActionResult<Guid>> Create
        ([FromBody] AddPlantDto2 body, CancellationToken token)
    {
        var meta = _metadataFactory.Create<AddToStockCommand>(new(Guid.NewGuid(), nameof(PlantStock)));
        var command = new AddToStockCommand(meta, new PlantStockDto(body.Name));
        var result = await _sender.SendCommandAsync(command);
        return command.Metadata.Aggregate.Id;
    }

}

public record AddPlantDto2(string Name, string Description, int[] Regions, int SoilId, int GroupId, DateTime Created, byte[][] Pictures);
