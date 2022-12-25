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
        return aggregate.RequireNew() switch
        {
            null => repository.InsertAsync(aggregate),
            CommandForbidden _ => repository.UpdateAsync(aggregate)
        };
    }
}