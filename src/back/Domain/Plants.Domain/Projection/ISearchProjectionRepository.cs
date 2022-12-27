namespace Plants.Domain.Projection;

public interface ISearchProjectionRepository<T>
{
    Task IndexAsync(T item);
}
