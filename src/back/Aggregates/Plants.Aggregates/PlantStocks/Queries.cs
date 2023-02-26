namespace Plants.Aggregates;

internal sealed class GetStockItemsHandler : IRequestHandler<GetStockItems, IEnumerable<StockViewResultItem>>
{
    private readonly IIdentityProvider _userIdentity;
    private readonly ISearchQueryService<PlantStock, PlantStockParams> _search;

    public GetStockItemsHandler(IIdentityProvider userIdentity, ISearchQueryService<PlantStock, PlantStockParams> search)
    {
        _userIdentity = userIdentity;
        _search = search;
    }

    public async Task<IEnumerable<StockViewResultItem>> Handle(GetStockItems request, CancellationToken cancellationToken)
    {
        var result = await _search.SearchAsync(new PlantStockParams(false), new QueryOptions.All(), cancellationToken);
        return result.Select(stock => new StockViewResultItem(stock.Id, stock.Information.PlantName, stock.Information.Description, stock.Caretaker.Login == _userIdentity.Identity.UserName));
    }
}

internal sealed class GetStockItemHandler : IRequestHandler<GetStockItem, PlantViewResultItem?>
{
    private readonly IProjectionQueryService<PlantStock> _query;

    public GetStockItemHandler(IProjectionQueryService<PlantStock> query)
    {
        _query = query;
    }

    public async Task<PlantViewResultItem?> Handle(GetStockItem request, CancellationToken cancellationToken)
    {
        PlantViewResultItem? item;
        if (await _query.ExistsAsync(request.StockId, cancellationToken))
        {
            var plant = await _query.GetByIdAsync(request.StockId, cancellationToken);
            var info = plant.Information;
            item = new PlantViewResultItem(info.PlantName, info.Description,
                info.GroupNames, info.SoilNames, plant.Pictures, info.RegionNames, plant.CreatedTime);
        }
        else
        {
            item = null;
        }
        return item;
    }
}

internal sealed class GetPreparedHandler : IRequestHandler<GetPrepared, PreparedPostResultItem?>
{
    private readonly IProjectionQueryService<PlantStock> _stockQuery;
    private readonly IProjectionQueryService<User> _userQuery;
    private readonly IIdentityProvider _identityProvider;

    public GetPreparedHandler(IProjectionQueryService<PlantStock> stockQuery, IProjectionQueryService<User> userQuery, IIdentityProvider identityProvider)
    {
        _stockQuery = stockQuery;
        _userQuery = userQuery;
        _identityProvider = identityProvider;
    }

    public async Task<PreparedPostResultItem?> Handle(GetPrepared request, CancellationToken token)
    {
        PreparedPostResultItem? result;
        var userId = _identityProvider.Identity!.UserName.ToGuid();
        if (await _stockQuery.ExistsAsync(request.StockId, token) && await _userQuery.ExistsAsync(userId, token))
        {
            var stock = await _stockQuery.GetByIdAsync(request.StockId, token);
            var seller = await _userQuery.GetByIdAsync(userId, token);
            var caretaker = stock.Caretaker;
            var plant = stock.Information;
            result = new PreparedPostResultItem(stock.Id,
                plant.PlantName, plant.Description,
                plant.SoilNames, plant.RegionNames, plant.GroupNames, stock.CreatedTime,
                seller.FullName, seller.PhoneNumber, seller.PlantsCared, seller.PlantsSold, seller.InstructionCreated,
                caretaker.PlantsCared, caretaker.PlantsSold, caretaker.InstructionCreated,
                stock.Pictures
                );
        }
        else
        {
            result = null;
        }
        return result;
    }
}
