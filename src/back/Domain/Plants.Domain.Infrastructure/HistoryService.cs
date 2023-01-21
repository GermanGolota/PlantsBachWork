using Plants.Domain.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants.Domain.History;

internal class HistoryService : IHistoryService
{
    private readonly IEventStore _store;
    private readonly AggregateEventApplyer _applyer;

    public HistoryService(IEventStore store, AggregateEventApplyer applyer)
    {
        _store = store;
        _applyer = applyer;
    }

    public async Task<HistoryModel> GetAsync(AggregateDescription aggregate, CancellationToken token)
    {
        var events = await _store.ReadEventsAsync(aggregate, token);
        var processed = _applyer.ApplyEvents(aggregate, events);

        var allEvents = events
            .SelectMany(_ =>
            {
                var result = new List<OneOf<Event, Command>>() { _.Command };
                result.AddRange(_.Events.Select(_ => new OneOf<Event, Command>(_)));
                return result;
            })
            .ToList();
        return new(allEvents, processed.Referenced);
    }
}
