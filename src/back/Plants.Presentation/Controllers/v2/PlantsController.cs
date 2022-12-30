using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantStocks;
using Plants.Aggregates.Search;
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
    private readonly IIdentityProvider _identity;
    private readonly ISearchQueryService<PlantStock, PlantStockParams> _search;

    public PlantsController(CommandMetadataFactory metadataFactory, ICommandSender sender, IIdentityProvider identity, ISearchQueryService<PlantStock, PlantStockParams> search)
    {
        _metadataFactory = metadataFactory;
        _sender = sender;
        _identity = identity;
        _search = search;
    }

    [HttpPost("add")]
    [SwaggerRequestExample(typeof(PlantStockDto), typeof(AddPlantRequestExample))]
    [ApiVersion("2")]
    public async Task<IActionResult> Create([FromForm] AddPlantDto2 request, CancellationToken token)
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

    [HttpGet("notposted")]
    [ApiVersion("2")]
    public async Task<ActionResult<PlantsResultDto>> GetNotPosted(CancellationToken token)
    {
        //todo: add filtering
        var username = _identity.Identity!.UserName;
        var result = await _search.SearchAsync(new PlantStockParams(false), new SearchAll());
        return new PlantsResultDto(
                result
                    .Select(stock => MapPlant(stock, username))
                    .ToList());
    }

    private static PlantResultItemDto MapPlant(PlantStock stock, string username) =>
        new(stock.Id, stock.PlantName, stock.Description, stock.CaretakerUsername == username);

}

public record AddPlantDto2(PlantStockDto Plant, List<IFormFile> Files);
public record PlantsResultDto(List<PlantResultItemDto> Items);
public record PlantResultItemDto(Guid Id, string PlantName, string Description, bool IsMine);