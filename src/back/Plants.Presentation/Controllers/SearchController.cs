using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantInfos;
using Plants.Aggregates.PlantPosts;
using Plants.Aggregates.Search;
using Plants.Application.Requests;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("search")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<ActionResult<SearchResult>> Stats
        ([FromQuery] SearchRequest request, CancellationToken token)
    {
        var res = await _mediator.Send(request, token);
        return Ok(res);
    }
}

[ApiController]
[Route("v2/search")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class SearchControllerV2 : ControllerBase
{
    private readonly ISearchQueryService<PlantPost, PlantPostParams> _search;
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;

    public SearchControllerV2(
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
        var param = new PlantPostParams(request.PlantName, request.LowerPrice, request.TopPrice, request.LastDate, groups, regions, soils);
        var result = await _search.SearchAsync(param, new SearchAll(), token);

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
