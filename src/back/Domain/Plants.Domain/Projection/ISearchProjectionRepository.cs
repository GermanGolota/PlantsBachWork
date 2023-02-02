namespace Plants.Domain;

public interface ISearchProjectionRepository<T>
{
    Task IndexAsync(T item, CancellationToken token = default);
}
