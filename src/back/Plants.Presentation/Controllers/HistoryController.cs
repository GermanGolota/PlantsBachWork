using Humanizer;
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
    public async Task<ActionResult<HistoryViewModel>> GetHistory([FromQuery] string name, [FromQuery] Guid id, CancellationToken token)
    {
        var model = await _history.GetAsync(new(id, name), token);
        return new HistoryViewModel(model.Snapshots.Select(_ => new AggregateSnapshotViewModel(_, _.Time.Humanize(utcDate: true))).ToList());
    }

    public record HistoryViewModel(List<AggregateSnapshotViewModel> Snapshots);
    public record AggregateSnapshotViewModel(AggregateSnapshot Snapshot, string HumanTime);
}
