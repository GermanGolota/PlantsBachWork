namespace Plants.Aggregates;

internal sealed class SearchPostsHandler : IRequestHandler<SearchPosts, IEnumerable<PostSearchViewResultItem>>
{
    private readonly ISearchQueryService<PlantPost, PlantPostParams> _search;

    public SearchPostsHandler(ISearchQueryService<PlantPost, PlantPostParams> search)
    {
        _search = search;
    }

    public async Task<IEnumerable<PostSearchViewResultItem>> Handle(SearchPosts request, CancellationToken token)
    {
        var result = await _search.SearchAsync(request.Parameters, new QueryOptions.All(), token);
        var parameters = request.Parameters;
        //TODO: Fix group filtering not working with elastic
        result = result.Where(post => (
                parameters.GroupNames is null || post.Stock.Information.GroupNames.Intersect(parameters.GroupNames).Any())
            && (parameters.RegionNames is null || post.Stock.Information.RegionNames.Intersect(parameters.RegionNames).Any())
            && (parameters.SoilNames is null || post.Stock.Information.SoilNames.Intersect(parameters.SoilNames).Any()
            )
        );
        return result.Select(item =>
            new PostSearchViewResultItem(
                item.Id,
                item.Stock.Information.PlantName,
                item.Stock.Information.Description,
                item.Stock.Pictures,
                (double)item.Price)
            );
    }
}

internal sealed class GetPostHandler : IRequestHandler<GetPost, PostViewResultItem?>
{
    private readonly IProjectionQueryService<PlantPost> _query;

    public GetPostHandler(IProjectionQueryService<PlantPost> query)
    {
        _query = query;
    }

    public async Task<PostViewResultItem?> Handle(GetPost request, CancellationToken token)
    {
        PostViewResultItem? result;
        if (await _query.ExistsAsync(request.PostId, token))
        {
            var post = await _query.GetByIdAsync(request.PostId, token);
            if (post.IsRemoved)
            {
                result = null;
            }
            else
            {
                var seller = post.Seller;
                var stock = post.Stock;
                var caretaker = stock.Caretaker;
                var plant = stock.Information;
                result = new(post.Id, plant.PlantName, plant.Description, post.Price,
                    plant.SoilNames, plant.RegionNames, plant.GroupNames, stock.CreatedTime,
                    seller.FullName, seller.PhoneNumber, seller.PlantsCared, seller.PlantsSold, seller.InstructionCreated,
                    caretaker.PlantsCared, caretaker.PlantsSold, caretaker.InstructionCreated,
                    stock.Pictures);
            }
        }
        else
        {
            result = null;
        }
        return result;
    }
}
