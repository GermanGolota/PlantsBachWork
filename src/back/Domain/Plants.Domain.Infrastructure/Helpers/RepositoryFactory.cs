using Microsoft.Extensions.DependencyInjection;
using Plants.Domain.Persistence;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure.Helpers;

internal class RepositoryFactory
{
    private readonly AggregateHelper _aggregate;
    private readonly IServiceProvider _service;

    public RepositoryFactory(AggregateHelper aggregate, IServiceProvider service)
    {
        _aggregate = aggregate;
        _service = service;
    }

    public Func<Guid, Task<AggregateBase>> CreateFor(string aggregateName)
    {
        var aggregateType = _aggregate.Aggregates[aggregateName];
        var repositoryType = typeof(IRepository<>).MakeGenericType(aggregateType);
        var repository = _service.GetRequiredService(repositoryType);
        var method = repository.GetType().GetMethod(nameof(IRepository<AggregateBase>.GetByIdAsync));
        return async (id) => (AggregateBase)await (dynamic)method.Invoke(repository, new object[] { id });
    }
}
