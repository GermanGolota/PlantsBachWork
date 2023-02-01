namespace Plants.Domain.Persistence;

public interface IRepository<TAggregate> where TAggregate : AggregateBase
{
    Task<TAggregate> GetByIdAsync(Guid id, DateTime? asOf = null, CancellationToken token = default);
}
