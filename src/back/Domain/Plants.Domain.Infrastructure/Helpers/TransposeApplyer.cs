﻿namespace Plants.Domain.Infrastructure.Helpers;

internal class TransposeApplyer<TIn> where TIn : AggregateBase
{
    private readonly IRepository<TIn> _repo;

    public TransposeApplyer(IRepository<TIn> repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Event>> CallTransposeAsync(AggregateLoadingTranspose<TIn> transpose, IEnumerable<Event> events)
    {
        var id = transpose.ExtractId(events.First());
        var aggregate = await _repo.GetByIdAsync(id);
        return events.Select(@event => transpose.Transpose(@event, aggregate));
    }
}

internal class TransposeApplyer<TIn, TEvent> where TIn : AggregateBase where TEvent : Event
{
    private readonly IRepository<TIn> _repo;

    public TransposeApplyer(IRepository<TIn> repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Event>> CallTransposeAsync(AggregateLoadingTranspose<TIn, TEvent> transpose, IEnumerable<Event> events)
    {
        var filteredEvents = events.OfType<TEvent>();
        var id = transpose.ExtractId(filteredEvents.First());
        var aggregate = await _repo.GetByIdAsync(id);
        return filteredEvents.Select(@event => transpose.Transpose(@event, aggregate));
    }
}
