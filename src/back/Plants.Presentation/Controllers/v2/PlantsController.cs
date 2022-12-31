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
    private readonly CommandHelper _command;
    private readonly ISearchQueryService<PlantStock, PlantStockParams> _search;

    public PlantsController(CommandHelper command, ISearchQueryService<PlantStock, PlantStockParams> search)
    {
        _command = command;
        _search = search;
    }

    [HttpPost("add")]
    [SwaggerRequestExample(typeof(PlantInformation), typeof(AddPlantRequestExample))]
    [ApiVersion("2")]
    public async Task<IActionResult> Create([FromForm] AddPlantDto2 request, CancellationToken token)
    {
        var pictures = await Task.WhenAll(request.Files.Select(file => file.ReadBytesAsync()));
        var stockId = Guid.NewGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<AddToStockCommand>(new(stockId, nameof(PlantStock))),
            meta => new AddToStockCommand(meta, request.Plant, pictures)
            );

        return result.Match<IActionResult>(
            success => Ok(stockId),
            failure => BadRequest(failure.Reasons)
            );
    }

    [HttpGet("notposted")]
    [ApiVersion("2")]
    public async Task<ActionResult<PlantsResultDto>> GetNotPosted(CancellationToken token)
    {
        //todo: add filtering
        var username = _command.IdentityProvider.Identity!.UserName;
        var result = await _search.SearchAsync(new PlantStockParams(false), new SearchAll());
        return new PlantsResultDto(
                result
                    .Select(stock => MapPlant(stock, username))
                    .ToList());
    }

    private static PlantResultItemDto MapPlant(PlantStock stock, string username) =>
        new(stock.Id, stock.Information.PlantName, stock.Information.Description, stock.CaretakerUsername == username);

}

public record AddPlantDto2(PlantInformation Plant, List<IFormFile> Files);
public record PlantsResultDto(List<PlantResultItemDto> Items);
public record PlantResultItemDto(Guid Id, string PlantName, string Description, bool IsMine);