using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("plants")]
public class PlantsController : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly IMediator _query;
    private readonly IPictureUploader _fileUploader;

    public PlantsController(CommandHelper command,
        IMediator query,
        IPictureUploader fileUploader)
    {
        _command = command;
        _query = query;
        _fileUploader = fileUploader;
    }

    [HttpGet("notposted")]
    public async Task<ActionResult<ListViewResult<StockViewResultItem>>> GetNotPosted(CancellationToken token)
    {
        var items = await _query.Send(new GetStockItems(new PlantStockParams(false), new QueryOptions.All()), token);
        return new ListViewResult<StockViewResultItem>(items);
    }

    [HttpGet("notposted/{id}")]
    public async Task<ActionResult<QueryViewResult<PlantViewResultItem>>> GetNotPosted([FromRoute] Guid id, CancellationToken token)
    {
        var item = await _query.Send(new GetStockItem(id), token);
        return item.ToQueryResult();
    }

    [HttpGet("prepared/{id}")]
    public async Task<ActionResult<QueryViewResult<PreparedPostResultItem>>> GetPrepared(Guid id, CancellationToken token)
    {
        var item = await _query.Send(new GetPrepared(id), token);
        return item.ToQueryResult();
    }

    [HttpPost("{id}/post")]
    public async Task<ActionResult<CommandViewResult>> Post([FromRoute] Guid id,
        [FromQuery] decimal price, CancellationToken token)
    {
        var result = await _command.SendAndNotifyAsync(
            factory => factory.Create<PostStockItemCommand, PlantStock>(id),
            meta => new PostStockItemCommand(meta, price),
            token);
        return result.ToCommandResult();
    }

    public record AddPlantViewRequest(string Name, string Description, 
        string[] RegionNames, string[] SoilNames, string[] FamilyNames, DateTime Created);

    [HttpPost("add")]
    public async Task<ActionResult<CommandViewResult>> Create
        ([FromForm] AddPlantViewRequest body, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var bytes = await Task.WhenAll(files.Select(file => file.ReadBytesAsync(token)));
        var pictures = await _fileUploader.UploadAsync(token, bytes.Select(_ => new FileView(Guid.NewGuid(), _)).ToArray());
        var stockId = new Random().GetRandomConvertableGuid();
        var plantInfo = new PlantInformation(body.Name, body.Description, body.RegionNames, body.SoilNames, body.FamilyNames);
        var result = await _command.SendAndNotifyAsync(
            factory => factory.Create<AddToStockCommand>(new(stockId, nameof(PlantStock))),
            meta => new AddToStockCommand(meta, plantInfo, body.Created, pictures),
            token
            );

        return result.ToCommandResult();
    }

    public record EditPlantViewRequest(string PlantName,
      string PlantDescription, string[] RegionNames, string[] SoilNames, string[] FamilyNames, Guid[]? RemovedImages);

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<CommandViewResult>> Edit
      ([FromRoute] Guid id, [FromForm] EditPlantViewRequest plant, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var bytes = await Task.WhenAll(files.Select(file => file.ReadBytesAsync(token)));
        var pictures = await _fileUploader.UploadAsync(token, bytes.Select(_ => new FileView(Guid.NewGuid(), _)).ToArray());

        var plantInfo = new PlantInformation(plant.PlantName, plant.PlantDescription, plant.RegionNames, plant.SoilNames, plant.FamilyNames);
        var result = await _command.SendAndNotifyAsync(
            factory => factory.Create<EditStockItemCommand>(new(id, nameof(PlantStock))),
            meta => new EditStockItemCommand(meta, plantInfo, pictures, plant.RemovedImages ?? Array.Empty<Guid>()),
            token);
        return result.ToCommandResult();
    }

}