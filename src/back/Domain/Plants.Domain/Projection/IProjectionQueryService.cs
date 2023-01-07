using System.Linq.Expressions;

namespace Plants.Domain.Projection;

public interface IProjectionQueryService<T>
{
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default);
    Task<T> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken token = default);
}
