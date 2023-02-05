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

    public record SearchViewRequest(string? PlantName,
        decimal? LowerPrice,
        decimal? TopPrice,
        DateTime? LastDate,
        string[]? GroupNames,
        string[]? RegionNames,
        string[]? SoilNames);

    public record SearchViewResultItem(Guid Id, string PlantName, string Description, Picture[] Images, double Price);

    [HttpGet("")]
    public async Task<ActionResult<ListViewResult<SearchViewResultItem>>> Search
        ([FromQuery] SearchViewRequest request, CancellationToken token)
    {
        var param = new PlantPostParams(request.PlantName, request.LowerPrice, request.TopPrice, request.LastDate/*, groups, regions, soils*/);
        var result = await _search.SearchAsync(param, new SearchAll(), token);
        //TODO: Fix group filtering not working with elastic
        result = result.Where(post => (
                request.GroupNames is null || post.Stock.Information.GroupNames.Intersect(request.GroupNames).Any())
            && (request.RegionNames is null || post.Stock.Information.RegionNames.Intersect(request.RegionNames).Any())
            && (request.SoilNames is null || post.Stock.Information.SoilNames.Intersect(request.SoilNames).Any()
            )
        );
        return new ListViewResult<SearchViewResultItem>(result.Select(item =>
            new SearchViewResultItem(
                item.Id,
                item.Stock.Information.PlantName,
                item.Stock.Information.Description,
                item.Stock.Pictures,
                (double)item.Price)).ToList()
                );
    }
}
