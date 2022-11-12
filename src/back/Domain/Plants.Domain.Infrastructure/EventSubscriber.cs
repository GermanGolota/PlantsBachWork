using Plants.Domain.Infrastructure.Helpers;

namespace Plants.Domain.Infrastructure;

internal class EventSubscriber
{
    private readonly RepositoryCaller _caller;

    public EventSubscriber(RepositoryCaller caller)
    {
        _caller = caller;
    }

    public async Task UpdateAggregateAsync(AggregateDescription desc, IEnumerable<Event> newEvents)
    {
        var aggregate = await _caller.LoadAsync(desc);
        if (aggregate.Version == 0)
        {
            await _caller.CreateAsync(aggregate);
        }
        else
        {
            await _caller.UpdateAsync(aggregate);
        }
    }
}
