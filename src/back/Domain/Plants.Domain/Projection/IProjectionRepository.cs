using Plants.Domain.Aggregate;

namespace Plants.Domain.Projection;

public interface IProjectionRepository<T>
{
    Task InsertAsync(T entity, CancellationToken token = default);

    Task UpdateAsync(T entity, CancellationToken token = default);
}

public static class ProjectionRepositoryExtensions
{
    public static async Task InsertOrUpdateAsync<TAggregate>(this IProjectionRepository<TAggregate> repository, TAggregate aggregate, CancellationToken token = default) where TAggregate : AggregateBase
    {
        var exists = aggregate.RequireNew() is null;
        try
        {
            if (exists)
            {
                await repository.InsertAsync(aggregate, token);
            }
            else
            {
                await repository.UpdateAsync(aggregate, token);
            }
        }
        catch (RepositoryException exc)
        {
            if (exists && exc.ErrorCode == RepositoryErrorCode.NotFound)
            {
                await repository.InsertAsync(aggregate, token);
            }
            else
            {
                if(exc.ErrorCode == RepositoryErrorCode.AlreadyExists)
                {
                    await repository.UpdateAsync(aggregate, token);
                }
            }
        }
    }
}