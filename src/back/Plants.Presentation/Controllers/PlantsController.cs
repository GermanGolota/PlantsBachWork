using Humanizer;
using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("plants")]
public class PlantsController : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly ISearchQueryService<PlantStock, PlantStockParams> _search;
    private readonly IProjectionQueryService<PlantStock> _stockProjector;
    private readonly IProjectionQueryService<User> _userProjector;

    public PlantsController(CommandHelper command,
        ISearchQueryService<PlantStock, PlantStockParams> search,
        IProjectionQueryService<PlantStock> stockProjector,
        IProjectionQueryService<User> userProjector)
    {
        _command = command;
        _search = search;
        _stockProjector = stockProjector;
        _userProjector = userProjector;
    }

    public record StockViewResultItem(Guid Id, string PlantName, string Description, bool IsMine);

    [HttpGet("notposted")]
    public async Task<ActionResult<ListViewResult<StockViewResultItem>>> GetNotPosted(CancellationToken token)
    {
        var username = _command.IdentityProvider.Identity!.UserName;
        var result = await _search.SearchAsync(new PlantStockParams(false), new SearchAll(), token);
        return new ListViewResult<StockViewResultItem>(
                result.Select(stock =>
                new StockViewResultItem(stock.Id, stock.Information.PlantName, stock.Information.Description, stock.Caretaker.Login == username)
                ));
    }

    public record PlantViewResultItem(string PlantName, string Description, string[] GroupNames,
        string[] SoilNames, Picture[] Images, string[] RegionNames, DateTime Created)
    {
        public string CreatedHumanDate => Created.Humanize();
        public string CreatedDate => Created.ToShortDateString();
    }

    [HttpGet("notposted/{id}")]
    public async Task<ActionResult<QueryViewResult<PlantViewResultItem>>> GetNotPosted([FromRoute] Guid id, CancellationToken token)
    {
        QueryViewResult<PlantViewResultItem> result;
        if (await _stockProjector.ExistsAsync(id, token))
        {
            var plant = await _stockProjector.GetByIdAsync(id, token);
            var info = plant.Information;
            result = new(new PlantViewResultItem(info.PlantName, info.Description,
                info.GroupNames, info.SoilNames, plant.Pictures, info.RegionNames, plant.CreatedTime));
        }
        else
        {
            result = new();
        }
        return result;
    }

    public record PreparedPostResultItem2(
        Guid Id, string PlantName, string Description, string[] SoilNames,
        string[] RegionNames, string[] GroupNames, DateTime Created,
        string SellerName, string SellerPhone, long SellerCared, long SellerSold, long SellerInstructions,
        long CareTakerCared, long CareTakerSold, long CareTakerInstructions, Picture[] Images)
    {
        public string CreatedHumanDate => Created.Humanize();
        public string CreatedDate => Created.ToShortDateString();
    }

    [HttpGet("prepared/{id}")]
    public async Task<ActionResult<QueryViewResult<PreparedPostResultItem2>>> GetPrepared(Guid id, CancellationToken token)
    {
        var userId = _command.IdentityProvider.Identity!.UserName.ToGuid();
        QueryViewResult<PreparedPostResultItem2> result;
        if (await _stockProjector.ExistsAsync(id, token) && await _userProjector.ExistsAsync(userId, token))
        {
            var stock = await _stockProjector.GetByIdAsync(id, token);
            var seller = await _userProjector.GetByIdAsync(userId, token);
            var caretaker = stock.Caretaker;
            var plant = stock.Information;
            result = new(new PreparedPostResultItem2(stock.Id,
                plant.PlantName, plant.Description,
                plant.SoilNames, plant.RegionNames, plant.GroupNames, stock.CreatedTime,
                seller.FullName, seller.PhoneNumber, seller.PlantsCared, seller.PlantsSold, seller.InstructionCreated,
                caretaker.PlantsCared, caretaker.PlantsSold, caretaker.InstructionCreated,
                stock.Pictures
                ));
        }
        else
        {
            result = new();
        }

        return result;
    }

    [HttpPost("{id}/post")]
    public async Task<ActionResult<CommandViewResult>> Post([FromRoute] Guid id,
        [FromQuery] decimal price, CancellationToken token)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<PostStockItemCommand, PlantStock>(id),
            meta => new PostStockItemCommand(meta, price),
            wait: true, 
            token);
        return result.ToCommandResult();
    }

    public record AddPlantViewRequest(string Name, string Description, 
        string[] RegionNames, string[] SoilNames, string[] GroupNames, DateTime Created);

    [HttpPost("add")]
    public async Task<ActionResult<CommandViewResult>> Create
        ([FromForm] AddPlantViewRequest body, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var pictures = await Task.WhenAll(files.Select(file => file.ReadBytesAsync(token)));
        var stockId = new Random().GetRandomConvertableGuid();
        var plantInfo = new PlantInformation(body.Name, body.Description, body.RegionNames, body.SoilNames, body.GroupNames);
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<AddToStockCommand>(new(stockId, nameof(PlantStock))),
            meta => new AddToStockCommand(meta, plantInfo, body.Created, pictures),
            wait: false,
            token
            );

        return result.ToCommandResult();
    }

    public record EditPlantViewRequest(string PlantName,
      string PlantDescription, string[] RegionNames, string[] SoilNames, string[] GroupNames, Guid[]? RemovedImages);

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<CommandViewResult>> Edit
      ([FromRoute] Guid id, [FromForm] EditPlantViewRequest plant, IEnumerable<IFormFile> files, CancellationToken token)
    {
        var pictures = await Task.WhenAll(files.Select(file => file.ReadBytesAsync(token)));
        var plantInfo = new PlantInformation(plant.PlantName, plant.PlantDescription, plant.RegionNames, plant.SoilNames, plant.GroupNames);
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<EditStockItemCommand>(new(id, nameof(PlantStock))),
            meta => new EditStockItemCommand(meta, plantInfo, pictures, plant.RemovedImages),
            wait: false,
            token);
        return result.ToCommandResult();
    }

}