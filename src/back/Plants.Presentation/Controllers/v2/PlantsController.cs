using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.Plants;
using Plants.Domain;
using Plants.Domain.Services;
using Plants.Presentation.Examples;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("v2/plants")]
public class PlantsControllerV2 : ControllerBase
{
    private readonly CommandMetadataFactory _metadataFactory;
    private readonly ICommandSender _sender;

    public PlantsControllerV2(CommandMetadataFactory metadataFactory, ICommandSender sender)
    {
        _metadataFactory = metadataFactory;
        _sender = sender;
    }

    [HttpPost("add")]
    [SwaggerRequestExample(typeof(AddPlantDto), typeof(AddPlantRequestExample))]
    public async Task<ActionResult<Guid>> Create
        ([FromBody] AddPlantDto2 body, CancellationToken token)
    {
        var meta = _metadataFactory.Create<CreatePlantCommand>(new(Guid.NewGuid(), nameof(Plant)));
        var command = new CreatePlantCommand(meta, new PlantCreationDto(body.Name));
        var result = await _sender.SendCommandAsync(command);
        return command.Metadata.Aggregate.Id;
    }

}

public record AddPlantDto2(string Name, string Description, int[] Regions, int SoilId, int GroupId, DateTime Created, byte[][] Pictures);
