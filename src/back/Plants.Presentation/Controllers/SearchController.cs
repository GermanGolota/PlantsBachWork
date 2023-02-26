using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("search")]
public class SearchController : ControllerBase
{
    private readonly IMediator _query;

    public SearchController(IMediator query)
    {
        _query = query;
    }

    [HttpGet("")]
    public async Task<ActionResult<ListViewResult<PostSearchViewResultItem>>> Search
        ([FromQuery] PlantPostParams request, CancellationToken token)
    {
        var items = await _query.Send(new SearchPosts(request, new QueryOptions.All()), token);
        return new ListViewResult<PostSearchViewResultItem>(items.ToList());
    }
}
