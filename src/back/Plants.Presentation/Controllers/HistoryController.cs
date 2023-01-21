using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("v2/history")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _history;

    public HistoryController(IHistoryService history)
    {
        _history = history;
    }

    [HttpGet()]
    public async Task<ActionResult<HistoryModel>> GetHistory([FromQuery] string name, [FromQuery] Guid id, CancellationToken token)
    {
        return await _history.GetAsync(new(id, name), token);
    }
}
