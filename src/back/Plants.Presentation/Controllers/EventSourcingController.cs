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
        CancellationToken token = default)
    {
        var model = await _history.GetAsync(new(id, name), order, token);
        return new HistoryViewModel(model.Snapshots.Select(_ => new AggregateSnapshotViewModel(_, _.Time.Humanize(utcDate: true))).ToList());
    }

    [HttpGet("convert/{idType}/{id}")]
    public ActionResult<Guid> ConvertId(
        [FromRoute] string id,
        [FromRoute] IdConversionType idType,
        CancellationToken token) =>
            Ok(idType switch
            {
                IdConversionType.Guid => Guid.Parse(id),
                IdConversionType.String => id.ToGuid(),
                IdConversionType.Long => Int64.Parse(id).ToGuid(),
                _ => throw new NotImplementedException(),
            });

    public record HistoryViewModel(List<AggregateSnapshotViewModel> Snapshots);
    public record AggregateSnapshotViewModel(AggregateSnapshot Snapshot, string HumanTime);
}

public enum IdConversionType
{
    Guid = 0, String = 1, Long = 2
}