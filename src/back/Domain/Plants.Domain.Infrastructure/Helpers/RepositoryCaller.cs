using Microsoft.Extensions.DependencyInjection;
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
        if (_aggregate.Aggregates.TryGetFor(aggregate.Name, out var aggregateType))
        {
            var repositoryType = typeof(IRepository<>).MakeGenericType(aggregateType);
            var repository = _service.GetRequiredService(repositoryType);
            var method = repository.GetType().GetMethod(nameof(IRepository<AggregateBase>.GetByIdAsync));
            var aggValue = await (dynamic)method.Invoke(repository, new object[] { aggregate.Id })!;
            return (AggregateBase) aggValue;
        }

        throw new Exception("Aggregate was not found");
    }

    public async Task InsertOrUpdateProjectionAsync(AggregateBase aggregate)
    {
        var repository = GetProjectionRepository(aggregate, out var aggregateType);
        var method = typeof(ProjectionRepositoryExtensions).GetMethod(nameof(ProjectionRepositoryExtensions.InsertOrUpdateAsync));
        method = method!.MakeGenericMethod(new[] { aggregateType });
        await (Task)method.Invoke(null, new object[] { repository, aggregate });
    }

    private object GetProjectionRepository(AggregateBase aggregate, out Type aggregateType)
    {
        if (_aggregate.Aggregates.TryGetFor(aggregate.Name, out aggregateType))
        {
            var repositoryType = typeof(IProjectionRepository<>).MakeGenericType(aggregateType);
            return _service.GetRequiredService(repositoryType);
        }

        throw new Exception("Aggregate was not found");
    }

}