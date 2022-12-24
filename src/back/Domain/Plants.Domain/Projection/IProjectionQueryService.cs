using System.Linq.Expressions;

namespace Plants.Domain.Projection;

public interface IProjectionQueryService<T>
{
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate);
    Task<T> GetByIdAsync(Guid id);
    Task<bool> Exists(Guid id);
}
