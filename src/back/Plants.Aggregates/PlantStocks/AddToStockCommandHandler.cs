using Plants.Aggregates.Services;

namespace Plants.Aggregates.PlantStocks;

internal class AddToStockCommandHandler : ICommandHandler<AddToStockCommand>
{
    private readonly IFileRepository _file;
    private readonly IRepository<PlantStock> _repo;
    private PlantStock _stock = null;

    private const string _plantImageDirectory = "PlantImages";

    public AddToStockCommandHandler(IFileRepository file, IRepository<PlantStock> repo)
    {
        _file = file;
        _repo = repo;
    }

    public async Task<IEnumerable<Event>> HandleAsync(AddToStockCommand command)
    {
        _stock ??= await _repo.GetByIdAsync(command.Metadata.Aggregate.Id);
        var files = await Task.WhenAll(command.Pictures.Select(picture => _file.SaveAsync(new(GetNewFileLocation(), picture))));
        var urls = files.Select(_file.GetUrl).ToArray();
        return new[]
        {
            new StockAddedEvent(EventFactory.Shared.Create<StockAddedEvent>(command), command.Plant, urls, command.Metadata.UserName)
        };
    }

    public async Task<CommandForbidden?> ShouldForbidAsync(AddToStockCommand command, IUserIdentity userIdentity)
    {
        _stock ??= await _repo.GetByIdAsync(command.Metadata.Aggregate.Id);
        return userIdentity.HasRole(Producer).And(_stock.RequireNew);
    }

    private FileLocation GetNewFileLocation() =>
        new FileLocation(Path.Combine(_plantImageDirectory, _stock.Id.ToString()), Guid.NewGuid().ToString(), "jpeg");
}