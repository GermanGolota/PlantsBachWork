using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("search")]
public class SearchController : ControllerBase
{
    private readonly ISearchQueryService<PlantPost, PlantPostParams> _search;
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;

    public SearchController(
        ISearchQueryService<PlantPost, PlantPostParams> search,
        IProjectionQueryService<PlantInfo> infoQuery)
    {
        _search = search;
        _infoQuery = infoQuery;
    }

    [HttpGet("")]
    public async Task<ActionResult<SearchResult2>> Search
        ([FromQuery] SearchRequest request, CancellationToken token)
    {
        var info = await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token);
        var groups = request.GroupIds?.Select(id => info.GroupNames[id])?.ToArray();
        var regions = request.RegionIds?.Select(id => info.RegionNames[id])?.ToArray();
        var soils = request.SoilIds?.Select(id => info.SoilNames[id])?.ToArray();
        var images = info.PlantImagePaths.ToInverse();
        var param = new PlantPostParams(request.PlantName, request.LowerPrice, request.TopPrice, request.LastDate/*, groups, regions, soils*/);
        var result = await _search.SearchAsync(param, new SearchAll(), token);
        //TODO: Fix group filtering not working with elastic
        result = result.Where(post => (groups is null || post.Stock.Information.GroupName.In(groups))
        && (regions is null || post.Stock.Information.RegionNames.Intersect(regions).Any())
        && (soils is null || post.Stock.Information.SoilName.In(soils))
        );
        return new SearchResult2(result.Select(item =>
            new SearchResultItem2(
                item.Id,
                item.Stock.Information.PlantName,
                item.Stock.Information.Description,
                item.Stock.PictureUrls.Select(url => images[url].ToString()).ToArray(),
                (double)item.Price)).ToList()
                );
    }
}
