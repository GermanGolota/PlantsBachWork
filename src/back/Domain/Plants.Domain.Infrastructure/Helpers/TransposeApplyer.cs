using Plants.Domain.Persistence;

namespace Plants.Domain.Infrastructure.Helpers;

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
        return transpose.Transpose(events, aggregate);
    }
}
