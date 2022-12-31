using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantInfos;
using Plants.Aggregates.PlantStocks;
using Plants.Aggregates.Search;
using Plants.Application.Commands;
using Plants.Application.Requests;
using Plants.Presentation.Examples;
using Plants.Presentation.Extensions;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("plants")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class PlantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("notposted")]
    public async Task<ActionResult<PlantsResult>> GetNotPosted(CancellationToken token)
    {
        return await _mediator.Send(new PlantsRequest(), token);
    }

    [HttpGet("notposted/{id}")]
    public async Task<ActionResult<PlantResult>> GetNotPosted([FromRoute] int id, CancellationToken token)
    {
        return await _mediator.Send(new PlantRequest(id), token);
    }

    [HttpGet("prepared/{id}")]
    public async Task<ActionResult<PreparedPostResult>> GetPrepared(int id, CancellationToken token)
    {
        return await _mediator.Send(new PreparedPostRequest(id), token);
    }

    [HttpPost("{id}/post")]
    public async Task<ActionResult<CreatePostResult>> GetPrepared([FromRoute] int id,
        [FromQuery] decimal price, CancellationToken token)
    {
        return await _mediator.Send(new CreatePostCommand(id, price), token);
    }

    [HttpPost("add")]
    public async Task<ActionResult<AddPlantResult>> Create
        ([FromForm] AddPlantDto body, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var totalBytes = await ReadFilesAsync(files);

        var request = new AddPlantCommand(body.Name, body.Description,
            body.Regions, body.SoilId, body.GroupId, body.Created, totalBytes);

        return await _mediator.Send(request, token);
    }

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<EditPlantResult>> Edit
      ([FromRoute] long id, [FromForm] EditPlantDto plant, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var totalBytes = await ReadFilesAsync(files);

        var request = new EditPlantCommand(id, plant.PlantName, plant.PlantDescription,
            plant.RegionIds, plant.SoilId, plant.GroupId, plant.RemovedImages, totalBytes);
        return await _mediator.Send(request, token);
    }

    private static async Task<byte[][]> ReadFilesAsync(IEnumerable<IFormFile> files) =>
        await Task.WhenAll(files.Select(file => file.ReadBytesAsync()));

}

public record AddPlantDto(string Name, string Description, long[] Regions, long SoilId, long GroupId, DateTime Created);

public record EditPlantDto(string PlantName,
  string PlantDescription, long[] RegionIds, long SoilId, long GroupId, long[] RemovedImages);



[ApiController]
[Route("v2/plants")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class PlantsControllerV2 : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly ISearchQueryService<PlantStock, PlantStockParams> _search;
    private readonly IProjectionQueryService<PlantInfo> _infoProjector;

    public PlantsControllerV2(CommandHelper command,
        ISearchQueryService<PlantStock, PlantStockParams> search,
        IProjectionQueryService<PlantInfo> infoProjector)
    {
        _command = command;
        _search = search;
        _infoProjector = infoProjector;
    }

    [HttpGet("notposted")]
    public async Task<ActionResult<PlantsResult>> GetNotPosted(CancellationToken token)
    {
        //todo: add filtering
        var username = _command.IdentityProvider.Identity!.UserName;
        var result = await _search.SearchAsync(new PlantStockParams(false), new SearchAll());
        return new PlantsResult(
                result
                    .Select(stock => MapPlant(stock, username))
                    .ToList());
    }

    [HttpGet("notposted/{id}")]
    public async Task<ActionResult<PlantResult>> GetNotPosted([FromRoute] int id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    [HttpGet("prepared/{id}")]
    public async Task<ActionResult<PreparedPostResult>> GetPrepared(int id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    [HttpPost("{id}/post")]
    public async Task<ActionResult<CreatePostResult>> GetPrepared([FromRoute] int id,
        [FromQuery] decimal price, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    [HttpPost("add")]
    [ApiVersion("2")]
    public async Task<ActionResult<AddPlantResult>> Create
        ([FromForm] AddPlantDto body, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var pictures = await Task.WhenAll(files.Select(file => file.ReadBytesAsync()));
        var stockId = new Random().NextInt64().ToGuid();
        var info = await _infoProjector.GetByIdAsync(PlantInfo.InfoId);
        var regions = body.Regions.Select(_ => info.RegionNames[_.ToGuid()]).ToArray();
        var soil = info.SoilNames[body.SoilId.ToGuid()];
        var group = info.GroupNames[body.GroupId.ToGuid()];
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<AddToStockCommand>(new(stockId, nameof(PlantStock))),
            meta => new AddToStockCommand(meta, new(body.Name, body.Description, regions, soil, group, body.Created), pictures)
            );

        return result.Match<ActionResult<AddPlantResult>>(
            success => Ok(new AddPlantResult(stockId.ToLong())),
            failure => BadRequest(failure.Reasons)
            );
    }

    [HttpPost("add2")]
    [ApiVersion("2")]
    public async Task<ActionResult<AddPlantResult>> Create2
        ([FromForm] PlantInformation body, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var pictures = await Task.WhenAll(files.Select(file => file.ReadBytesAsync()));
        var stockId = new Random().NextInt64().ToGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<AddToStockCommand>(new(stockId, nameof(PlantStock))),
            meta => new AddToStockCommand(meta, body, pictures)
            );

        return result.Match<ActionResult<AddPlantResult>>(
            success => Ok(new AddPlantResult(stockId.ToLong())),
            failure => BadRequest(failure.Reasons)
            );
    }

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<EditPlantResult>> Edit
      ([FromRoute] int id, [FromForm] EditPlantDto plant, IEnumerable<IFormFile> files, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    private static PlantResultItem MapPlant(PlantStock stock, string username) =>
        new(stock.Id.ToLong(), stock.Information.PlantName, stock.Information.Description, stock.CaretakerUsername == username);

    private static async Task<byte[][]> ReadFilesAsync(IEnumerable<IFormFile> files) =>
        await Task.WhenAll(files.Select(file => file.ReadBytesAsync()));

}