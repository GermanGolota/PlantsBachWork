namespace Plants.Domain.Projection;

public interface IProjectionRepository<T>
{
    Task InsertAsync(T entity);

    Task UpdateAsync(T entity);
}

public static class ProjectionRepositoryExtensions
{
    public static Task InsertOrUpdateAsync<TAggregate>(this IProjectionRepository<TAggregate> repository, TAggregate aggregate) where TAggregate : AggregateBase
    {
        Task finalTask;
        var version = aggregate.Version;
        if (version is AggregateBase.NewAggregateVersion || version is AggregateBase.NewAggregateVersion + 1)
        {
            finalTask = repository.InsertAsync(aggregate);
        }
        else
        {
            finalTask = repository.UpdateAsync(aggregate);
        }
        return finalTask;
    }
}