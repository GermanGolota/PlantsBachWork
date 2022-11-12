using Microsoft.Extensions.DependencyInjection;
using Plants.Domain.Persistence;
using Plants.Domain.Projection;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure.Helpers;

internal class RepositoryCaller
{
    private readonly AggregateHelper _aggregate;
    private readonly IServiceProvider _service;

    public RepositoryCaller(AggregateHelper aggregate, IServiceProvider service)
    {
        _aggregate = aggregate;
        _service = service;
    }

    public async Task<AggregateBase> LoadAsync(AggregateDescription aggregate)
    {
        var aggregateType = _aggregate.Aggregates[aggregate.Name];
        var repositoryType = typeof(IRepository<>).MakeGenericType(aggregateType);
        var repository = _service.GetRequiredService(repositoryType);
        var method = repository.GetType().GetMethod(nameof(IRepository<AggregateBase>.GetByIdAsync));
        return (AggregateBase)await (dynamic)method.Invoke(repository, new object[] { aggregate.Id });
    }

    public async Task UpdateAsync(AggregateBase aggregate)
    {
        var repository = GetProjectionRepository(aggregate);
        var method = repository.GetType().GetMethod(nameof(IProjectionRepository<AggregateBase>.UpdateAsync));
        await (Task)method.Invoke(repository, new object[] { aggregate });
    }

    public async Task CreateAsync(AggregateBase aggregate)
    {
        var repository = GetProjectionRepository(aggregate);
        var method = repository.GetType().GetMethod(nameof(IProjectionRepository<AggregateBase>.InsertAsync));
        await (Task)method.Invoke(repository, new object[] { aggregate });
    }

    private object GetProjectionRepository(AggregateBase aggregate)
    {
        var aggregateType = _aggregate.Aggregates[aggregate.Name];
        var repositoryType = typeof(IProjectionRepository<>).MakeGenericType(aggregateType);
        return _service.GetRequiredService(repositoryType);
    }

}