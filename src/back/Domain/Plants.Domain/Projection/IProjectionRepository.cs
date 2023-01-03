namespace Plants.Domain.Projection;

public interface IProjectionRepository<T>
{
    Task InsertAsync(T entity);

    Task UpdateAsync(T entity);
}

public static class ProjectionRepositoryExtensions
{
    public static async Task InsertOrUpdateAsync<TAggregate>(this IProjectionRepository<TAggregate> repository, TAggregate aggregate) where TAggregate : AggregateBase
    {
        var exists = aggregate.RequireNew() is null;
        try
        {
            if (exists)
            {
                await repository.InsertAsync(aggregate);
            }
            else
            {
                await repository.UpdateAsync(aggregate);
            }
        }
        catch (RepositoryException exc)
        {
            if (exists && exc.ErrorCode == RepositoryErrorCode.NotFound)
            {
                await repository.InsertAsync(aggregate);
            }
            else
            {
                if(exc.ErrorCode == RepositoryErrorCode.AlreadyExists)
                {
                    await repository.UpdateAsync(aggregate);
                }
            }
        }
    }
}