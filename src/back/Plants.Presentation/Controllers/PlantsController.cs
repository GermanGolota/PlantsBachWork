using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantInfos;
using Plants.Aggregates.PlantStocks;
using Plants.Aggregates.Search;
using Plants.Aggregates.Users;
using Plants.Application.Commands;
using Plants.Application.Requests;
using Plants.Presentation.Extensions;

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
    public async Task<ActionResult<PlantResult>> GetNotPosted([FromRoute] long id, CancellationToken token)
    {
        return await _mediator.Send(new PlantRequest(id), token);
    }

    [HttpGet("prepared/{id}")]
    public async Task<ActionResult<PreparedPostResult>> GetPrepared(int id, CancellationToken token)
    {
        return await _mediator.Send(new PreparedPostRequest(id), token);
    }

    [HttpPost("{id}/post")]
    public async Task<ActionResult<CreatePostResult>> GetPrepared([FromRoute] long id,
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
    private readonly IProjectionQueryService<PlantStock> _stockProjector;
    private readonly IProjectionQueryService<User> _userProjector;

    public PlantsControllerV2(CommandHelper command,
        ISearchQueryService<PlantStock, PlantStockParams> search,
        IProjectionQueryService<PlantInfo> infoProjector,
        IProjectionQueryService<PlantStock> stockProjector,
        IProjectionQueryService<User> userProjector)
    {
        _command = command;
        _search = search;
        _infoProjector = infoProjector;
        _stockProjector = stockProjector;
        _userProjector = userProjector;
    }

    [HttpGet("notposted")]
    public async Task<ActionResult<PlantsResult>> GetNotPosted(CancellationToken token)
    {
        var username = _command.IdentityProvider.Identity!.UserName;
        var result = await _search.SearchAsync(new PlantStockParams(false), new SearchAll());
        return new PlantsResult(
                result
                    .Select(stock => MapPlant(stock, username))
                    .ToList());
    }

    [HttpGet("notposted/{id}")]
    public async Task<ActionResult<PlantResult>> GetNotPosted([FromRoute] long id, CancellationToken token)
    {
        var guid = id.ToGuid();
        PlantResult result;
        if (await _stockProjector.ExistsAsync(guid))
        {
            var plant = await _stockProjector.GetByIdAsync(guid);
            var images = (await _infoProjector.GetByIdAsync(PlantInfo.InfoId)).PlantImagePaths.ToInverse();
            var info = plant.Information;
            var dict = await _infoProjector.GetByIdAsync(PlantInfo.InfoId);
            var groups = dict.GroupNames.ToInverse();
            var soils = dict.SoilNames.ToInverse();
            var regions = dict.RegionNames.ToInverse();
            result = new PlantResult(new PlantResultDto(info.PlantName, info.Description,
                groups[info.GroupName], soils[info.SoilName], plant.PictureUrls.Select(url => images[url]).ToArray(), info.RegionNames.Select(_ => regions[_]).ToArray()));
        }
        else
        {
            result = new PlantResult();
        }
        return result;
    }

    [HttpGet("prepared/{id}")]
    public async Task<ActionResult<PreparedPostResult>> GetPrepared(long id, CancellationToken token)
    {
        var guid = id.ToGuid();
        var userId = _command.IdentityProvider.Identity!.UserName.ToGuid();
        PreparedPostResult result;
        if (await _stockProjector.ExistsAsync(guid) && await _userProjector.ExistsAsync(userId))
        {
            var stock = await _stockProjector.GetByIdAsync(guid);
            var seller = await _userProjector.GetByIdAsync(userId);
            var images = (await _infoProjector.GetByIdAsync(PlantInfo.InfoId)).PlantImagePaths.ToInverse();
            var caretaker = stock.Caretaker;
            var plant = stock.Information;
            result = new PreparedPostResult(new(stock.Id.ToLong(),
                plant.PlantName, plant.Description,
                plant.SoilName, plant.RegionNames, plant.GroupName, stock.CreatedTime,
                seller.FullName, seller.PhoneNumber, seller.PlantsCared, seller.PlantsSold, seller.InstructionCreated,
                caretaker.PlantsCared, caretaker.PlantsSold, caretaker.InstructionCreated, stock.PictureUrls.Select(url => images[url]).ToArray()
                ));
        }
        else
        {
            result = new();
        }

        return result;
    }

    [HttpPost("{id}/post")]
    public async Task<ActionResult<CreatePostResult>> Post([FromRoute] long id,
        [FromQuery] decimal price, CancellationToken token)
    {
        var guid = id.ToGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<PostStockItemCommand, PlantStock>(guid),
            meta => new PostStockItemCommand(meta, price));
        return result.Match(
            succ => new CreatePostResult(true, "Success"),
            fail => new CreatePostResult(false, String.Join('\n', fail.Reasons)));
    }

    [HttpPost("add")]
    [ApiVersion("2")]
    public async Task<ActionResult<AddPlantResult>> Create
        ([FromForm] AddPlantDto body, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var pictures = await Task.WhenAll(files.Select(file => file.ReadBytesAsync()));
        var stockId = new Random().GetRandomConvertableGuid();
        var info = await _infoProjector.GetByIdAsync(PlantInfo.InfoId);
        var regions = body.Regions.Select(regionId => info.RegionNames[regionId]).ToArray();
        var soil = info.SoilNames[body.SoilId];
        var group = info.GroupNames[body.GroupId];
        var plantInfo = new PlantInformation(body.Name, body.Description, regions, soil, group);
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<AddToStockCommand>(new(stockId, nameof(PlantStock))),
            meta => new AddToStockCommand(meta, plantInfo, body.Created, pictures)
            );

        return result.Match<ActionResult<AddPlantResult>>(
            success => Ok(new AddPlantResult(stockId.ToLong())),
            failure => BadRequest(failure.Reasons)
            );
    }

    [HttpPost("add2")]
    [ApiVersion("2")]
    public async Task<ActionResult<AddPlantResult>> Create2
        ([FromForm] PlantInformation body, DateTime created, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var pictures = await Task.WhenAll(files.Select(file => file.ReadBytesAsync()));
        var stockId = new Random().GetRandomConvertableGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<AddToStockCommand>(new(stockId, nameof(PlantStock))),
            meta => new AddToStockCommand(meta, body, created, pictures)
            );

        var resultId = stockId.ToLong();
        return result.Match<ActionResult<AddPlantResult>>(
            success => Ok(new AddPlantResult(resultId)),
            failure => BadRequest(failure.Reasons)
            );
    }

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<EditPlantResult>> Edit
      ([FromRoute] long id, [FromForm] EditPlantDto plant, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var pictures = await Task.WhenAll(files.Select(file => file.ReadBytesAsync()));
        var stockId = id.ToGuid();
        var info = await _infoProjector.GetByIdAsync(PlantInfo.InfoId);
        var regions = plant.RegionIds.Select(regionId => info.RegionNames[regionId]).ToArray();
        var soil = info.SoilNames[plant.SoilId];
        var group = info.GroupNames[plant.GroupId];
        var plantInfo = new PlantInformation(plant.PlantName, plant.PlantDescription, regions, soil, group);
        var removed = plant.RemovedImages.Select(image => info.PlantImagePaths[image]).ToArray();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<EditStockItemCommand>(new(stockId, nameof(PlantStock))),
            meta => new EditStockItemCommand(meta, plantInfo, pictures, removed));
        return new EditPlantResult();
    }

    private static PlantResultItem MapPlant(PlantStock stock, string username) =>
        new(stock.Id.ToLong(), stock.Information.PlantName, stock.Information.Description, stock.Caretaker.Login == username);

    private static async Task<byte[][]> ReadFilesAsync(IEnumerable<IFormFile> files) =>
        await Task.WhenAll(files.Select(file => file.ReadBytesAsync()));

}