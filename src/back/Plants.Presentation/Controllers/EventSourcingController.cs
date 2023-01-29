using Humanizer;
using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("v2/eventsourcing")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class EventSourcingController : ControllerBase
{
    private readonly IHistoryService _history;

    public EventSourcingController(IHistoryService history)
    {
        _history = history;
    }

    [HttpGet("history")]
    public async Task<ActionResult<HistoryViewModel>> GetHistory(
        [FromQuery] string name, 
        [FromQuery] Guid id,
        [FromQuery] OrderType order,
        CancellationToken token)
    {
        var model = await _history.GetAsync(new(id, name), order, token);
        return new HistoryViewModel(model.Snapshots.Select(_ => new AggregateSnapshotViewModel(_, _.Time.Humanize(utcDate: true))).ToList());
    }

    public record HistoryViewModel(List<AggregateSnapshotViewModel> Snapshots);
    public record AggregateSnapshotViewModel(AggregateSnapshot Snapshot, string HumanTime);
}
