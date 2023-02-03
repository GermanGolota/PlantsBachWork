using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

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
    public async Task<ActionResult<PlantsResult2>> GetNotPosted(CancellationToken token)
    {
        var username = _command.IdentityProvider.Identity!.UserName;
        var result = await _search.SearchAsync(new PlantStockParams(false), new SearchAll(), token);
        return new PlantsResult2(
                result
                    .Select(stock => MapPlant(stock, username))
                    .ToList());
    }
    

    [HttpGet("notposted/{id}")]
    public async Task<ActionResult<PlantResult2>> GetNotPosted([FromRoute] Guid id, CancellationToken token)
    {
        PlantResult2 result;
        if (await _stockProjector.ExistsAsync(id, token))
        {
            var plant = await _stockProjector.GetByIdAsync(id, token);
            var images = (await _infoProjector.GetByIdAsync(PlantInfo.InfoId, token)).PlantImagePaths.ToInverse();
            var info = plant.Information;
            var dict = await _infoProjector.GetByIdAsync(PlantInfo.InfoId, token);
            var groups = dict.GroupNames.ToInverse();
            var soils = dict.SoilNames.ToInverse();
            var regions = dict.RegionNames.ToInverse();
            result = new PlantResult2(new PlantResultDto2(info.PlantName, info.Description,
                groups[info.GroupName].ToString(), soils[info.SoilName].ToString(),
                plant.PictureUrls.Select(url => images[url].ToString()).ToArray(),
                info.RegionNames.Select(_ => regions[_].ToString()).ToArray())
            {
                Created = plant.CreatedTime
            });
        }
        else
        {
            result = new PlantResult2();
        }
        return result;
    }

    [HttpGet("prepared/{id}")]
    public async Task<ActionResult<PreparedPostResult2>> GetPrepared(Guid id, CancellationToken token)
    {
        var userId = _command.IdentityProvider.Identity!.UserName.ToGuid();
        PreparedPostResult2 result;
        if (await _stockProjector.ExistsAsync(id, token) && await _userProjector.ExistsAsync(userId, token))
        {
            var stock = await _stockProjector.GetByIdAsync(id, token);
            var seller = await _userProjector.GetByIdAsync(userId, token);
            var images = (await _infoProjector.GetByIdAsync(PlantInfo.InfoId, token)).PlantImagePaths.ToInverse();
            var caretaker = stock.Caretaker;
            var plant = stock.Information;
            result = new PreparedPostResult2(new(stock.Id,
                plant.PlantName, plant.Description,
                plant.SoilName, plant.RegionNames, plant.GroupName, stock.CreatedTime,
                seller.FullName, seller.PhoneNumber, seller.PlantsCared, seller.PlantsSold, seller.InstructionCreated,
                caretaker.PlantsCared, caretaker.PlantsSold, caretaker.InstructionCreated,
                stock.PictureUrls.Select(url => images[url].ToString()).ToArray()
                ));
        }
        else
        {
            result = new();
        }

        return result;
    }

    [HttpPost("{id}/post")]
    public async Task<ActionResult<CreatePostResult>> Post([FromRoute] Guid id,
        [FromQuery] decimal price, CancellationToken token)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<PostStockItemCommand, PlantStock>(id),
            meta => new PostStockItemCommand(meta, price),
            token);
        return result.Match(
            succ => new CreatePostResult(true, "Success"),
            fail => new CreatePostResult(false, String.Join('\n', fail.Reasons)));
    }

    [HttpPost("add")]
    [ApiVersion("2")]
    public async Task<ActionResult<AddPlantResult2>> Create
        ([FromForm] AddPlantDto body, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var pictures = await Task.WhenAll(files.Select(file => file.ReadBytesAsync(token)));
        var stockId = new Random().GetRandomConvertableGuid();
        var info = await _infoProjector.GetByIdAsync(PlantInfo.InfoId, token);
        var regions = body.Regions.Select(regionId => info.RegionNames[regionId]).ToArray();
        var soil = info.SoilNames[body.SoilId];
        var group = info.GroupNames[body.GroupId];
        var plantInfo = new PlantInformation(body.Name, body.Description, regions, soil, group);
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<AddToStockCommand>(new(stockId, nameof(PlantStock))),
            meta => new AddToStockCommand(meta, plantInfo, body.Created, pictures),
            token
            );

        return result.Match<ActionResult<AddPlantResult2>>(
            success => Ok(new AddPlantResult2(stockId)),
            failure => BadRequest(failure.Reasons)
            );
    }

    [HttpPost("add2")]
    [ApiVersion("2")]
    public async Task<ActionResult<AddPlantResult2>> Create2
        ([FromForm] PlantInformation body, DateTime created, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var pictures = await Task.WhenAll(files.Select(file => file.ReadBytesAsync(token)));
        var stockId = new Random().GetRandomConvertableGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<AddToStockCommand>(new(stockId, nameof(PlantStock))),
            meta => new AddToStockCommand(meta, body, created, pictures),
            token
            );

        return result.Match<ActionResult<AddPlantResult2>>(
            success => Ok(new AddPlantResult2(stockId)),
            failure => BadRequest(failure.Reasons)
            );
    }

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<EditPlantResult>> Edit
      ([FromRoute] Guid id, [FromForm] EditPlantDto plant, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var pictures = await Task.WhenAll(files.Select(file => file.ReadBytesAsync(token)));
        var info = await _infoProjector.GetByIdAsync(PlantInfo.InfoId, token);
        var regions = plant.RegionIds.Select(regionId => info.RegionNames[regionId]).ToArray();
        var soil = info.SoilNames[plant.SoilId];
        var group = info.GroupNames[plant.GroupId];
        var plantInfo = new PlantInformation(plant.PlantName, plant.PlantDescription, regions, soil, group);
        var removed = plant.RemovedImages?.Select(image => info.PlantImagePaths[image])?.ToArray() ?? Array.Empty<string>();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<EditStockItemCommand>(new(id, nameof(PlantStock))),
            meta => new EditStockItemCommand(meta, plantInfo, pictures, removed),
            token);
        return result.Match<EditPlantResult>(
            _ => new(true, "Successfull"),
            failure => new(false, String.Join("\n", failure.Reasons)));
    }

    private static PlantResultItem2 MapPlant(PlantStock stock, string username) =>
        new(stock.Id, stock.Information.PlantName, stock.Information.Description, stock.Caretaker.Login == username);

}