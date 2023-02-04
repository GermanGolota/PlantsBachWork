using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("search")]
public class SearchController : ControllerBase
{
    private readonly ISearchQueryService<PlantPost, PlantPostParams> _search;

    public SearchController(
        ISearchQueryService<PlantPost, PlantPostParams> search)
    {
        _search = search;
    }

    [HttpGet("")]
    public async Task<ActionResult<SearchResult2>> Search
        ([FromQuery] SearchRequest request, CancellationToken token)
    {
        var param = new PlantPostParams(request.PlantName, request.LowerPrice, request.TopPrice, request.LastDate/*, groups, regions, soils*/);
        var result = await _search.SearchAsync(param, new SearchAll(), token);
        //TODO: Fix group filtering not working with elastic
        result = result.Where(post => (request.GroupNames is null || post.Stock.Information.GroupName.In(request.GroupNames))
        && (request.RegionNames is null || post.Stock.Information.RegionNames.Intersect(request.RegionNames).Any())
        && (request.SoilNames is null || post.Stock.Information.SoilName.In(request.SoilNames))
        );
        return new SearchResult2(result.Select(item =>
            new SearchResultItem2(
                item.Id,
                item.Stock.Information.PlantName,
                item.Stock.Information.Description,
                item.Stock.Pictures,
                (double)item.Price)).ToList()
                );
    }
}
