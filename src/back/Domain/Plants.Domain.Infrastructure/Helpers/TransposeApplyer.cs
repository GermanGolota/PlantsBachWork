using Plants.Domain.Persistence;
using Plants.Shared;

namespace Plants.Domain.Infrastructure.Helpers;

internal class TransposeApplyer<TIn> where TIn : AggregateBase
{
    private readonly IRepository<TIn> _repo;

    public TransposeApplyer(IRepository<TIn> repo)
    {
        _repo = repo;
    }

    public Task<Event> CallTransposeAsync(OneOf<SimpleTranspose, AggregateLoadingTranspose<TIn>> transpose, Event @event) =>
        transpose.MatchAsync(simple => Task.FromResult(simple.Transpose(@event)), async loading =>
        {
            var id = loading.ExtractId(@event);
            var aggregate = await _repo.GetByIdAsync(id);
            return loading.Transpose(@event, aggregate);
        });
}
