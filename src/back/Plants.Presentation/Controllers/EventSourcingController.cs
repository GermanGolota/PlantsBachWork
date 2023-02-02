using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Plants.Domain.Abstractions;
using Plants.Domain.Infrastructure.Subscription;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("v2/eventsourcing")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class EventSourcingController : ControllerBase
{
    private readonly IHistoryService _history;
    private readonly IProjectionsUpdater _updater;
    private readonly IEventStore _eventStore;

    public EventSourcingController(IHistoryService history, IProjectionsUpdater updater, IEventStore eventStore)
    {
        _history = history;
        _updater = updater;
        _eventStore = eventStore;
    }

    [HttpGet("history")]
    public async Task<ActionResult<HistoryViewModel>> GetHistory(
        [FromQuery] string name,
        [FromQuery] Guid id,
        [FromQuery] OrderType order,
        [FromQuery] DateTime? time = null,
        CancellationToken token = default)
    {
        var model = await _history.GetAsync(new(id, name), order, time, token);
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

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAggregate(
        [FromQuery] string name,
        [FromQuery] Guid id,
        [FromQuery] DateTime? asOf,
        CancellationToken token
        )
    {
        var agg = await _updater.UpdateProjectionAsync(new(id, name), asOf, token);
        return Ok(agg);
    }

    [HttpPost("refreshAll")]
    public async Task<IActionResult> RefreshAll(CancellationToken token)
    {
        var streams = await _eventStore.GetStreamsAsync(token);
        var aggregates = streams.SelectMany(_ => _.Ids.Select(id => new AggregateDescription(id, _.AggregateName)));
        await Parallel.ForEachAsync(aggregates, new ParallelOptions()
        {
            CancellationToken = token,
            MaxDegreeOfParallelism = 10
        }, async (aggregate, token) =>
        {
            await _updater.UpdateProjectionAsync(aggregate, token: token);
        });
        return Ok();
    }

    public record HistoryViewModel(List<AggregateSnapshotViewModel> Snapshots);
    public record AggregateSnapshotViewModel(AggregateSnapshot Snapshot, string HumanTime);
}

public enum IdConversionType
{
    Guid = 0, String = 1, Long = 2
}