using Plants.Aggregates.Abstractions;

namespace Plants.Aggregates.PlantStocks;

internal class AddToStockCommandHandler : ICommandHandler<AddToStockCommand>
{
    private readonly FileUploader _uploader;
    private readonly IQueryService<PlantStock> _repo;
    private PlantStock _stock = null;

    private const string _plantImageDirectory = "PlantImages";

    public AddToStockCommandHandler(FileUploader uploader, IQueryService<PlantStock> repo)
    {
        _uploader = uploader;
        _repo = repo;
    }

    public async Task<IEnumerable<Event>> HandleAsync(AddToStockCommand command, CancellationToken token = default)
    {
        _stock ??= await _repo.GetByIdAsync(command.Metadata.Aggregate.Id, token: token);
        var urls = await _uploader.UploadPlantAsync(_stock.Id, command.Pictures, token);
        return new[]
        {
            new StockAddedEvent(EventFactory.Shared.Create<StockAddedEvent>(command), command.Plant, command.CreatedTime, urls, command.Metadata.UserName)
        };
    }

    public async Task<CommandForbidden?> ShouldForbidAsync(AddToStockCommand command, IUserIdentity userIdentity, CancellationToken token = default)
    {
        _stock ??= await _repo.GetByIdAsync(command.Metadata.Aggregate.Id, token: token);
        return userIdentity.HasRole(Producer).And(_stock.RequireNew);
    }

    private FileLocation GetNewFileLocation() =>
        new FileLocation(Path.Combine(_plantImageDirectory, _stock.Id.ToString()), Guid.NewGuid().ToString(), "jpeg");
}