using MediatR;
using Microsoft.AspNetCore.Mvc;
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
