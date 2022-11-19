namespace Plants.Domain.Projection;

public interface IProjectionRepository<T>
{
    Task InsertAsync(T entity);

    Task UpdateAsync(T entity);
}