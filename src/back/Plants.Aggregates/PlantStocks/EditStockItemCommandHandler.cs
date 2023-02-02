namespace Plants.Aggregates.PlantStocks;

internal class EditStockItemCommandHandler : ICommandHandler<EditStockItemCommand>
{
    private readonly IQueryService<PlantStock> _stockRepository;
    private readonly FileUploader _uploader;

    public EditStockItemCommandHandler(IQueryService<PlantStock> stockRepository, FileUploader uploader)
    {
        _stockRepository = stockRepository;
        _uploader = uploader;
    }

    private PlantStock _stock;

    public async Task<CommandForbidden?> ShouldForbidAsync(EditStockItemCommand command, IUserIdentity user, CancellationToken token = default)
    {
        _stock ??= await _stockRepository.GetByIdAsync(command.Metadata.Aggregate.Id, token: token);
        var validIdentity = user.HasRole(Manager).Or(user.HasRole(Producer).And(IsCaretaker(user, _stock)));
        var notPosted = (_stock.BeenPosted is false).ToForbidden("Cannot edit stock after it was posted");
        //TODO: Should validate data here
        return validIdentity.And(notPosted);
    }

    private CommandForbidden? IsCaretaker(IUserIdentity user, PlantStock plant) =>
        (user.UserName == plant.Caretaker.Login).ToForbidden("Cannot eddit somebody elses stock item");

    public async Task<IEnumerable<Event>> HandleAsync(EditStockItemCommand command, CancellationToken token = default)
    {
        _stock ??= await _stockRepository.GetByIdAsync(command.Metadata.Aggregate.Id, token: token);
        var newUrls = await _uploader.UploadPlantAsync(_stock.Id, command.NewPictures, token);
        return new[]
        {
            new StockEdditedEvent(EventFactory.Shared.Create<StockEdditedEvent>(command), command.Plant, newUrls, command.RemovedPictureUrls)
        };
    }

}
