using Microsoft.Extensions.DependencyInjection;
using Plants.Domain.Projection;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure.Helpers;

internal class RepositoriesCaller
{
    private readonly AggregateHelper _aggregate;
    private readonly IServiceProvider _service;

    public RepositoriesCaller(AggregateHelper aggregate, IServiceProvider service)
    {
        _aggregate = aggregate;
        _service = service;
    }

    public async Task<AggregateBase> LoadAsync(AggregateDescription aggregate, DateTime? asOf = null, CancellationToken token = default)
    {
        if (_aggregate.Aggregates.TryGetFor(aggregate.Name, out var aggregateType))
        {
            var repositoryType = typeof(IRepository<>).MakeGenericType(aggregateType);
            var repository = _service.GetRequiredService(repositoryType);
            var method = repository.GetType().GetMethod(nameof(IRepository<AggregateBase>.GetByIdAsync))!;
            var aggValue = await (dynamic)method.Invoke(repository, new object?[] { aggregate.Id, asOf, token })!;
            return (AggregateBase)aggValue;
        }

        throw new Exception("Aggregate was not found");
    }

    public async Task InsertOrUpdateProjectionAsync(AggregateBase aggregate, CancellationToken token = default)
    {
        var aggregateType = _aggregate.Aggregates.Get(aggregate.Metadata.Name);
        var repositoryType = typeof(IProjectionRepository<>).MakeGenericType(aggregateType);
        var repository = _service.GetRequiredService(repositoryType);
        var method = typeof(ProjectionRepositoryExtensions).GetMethod(nameof(ProjectionRepositoryExtensions.InsertOrUpdateAsync));
        method = method!.MakeGenericMethod(new[] { aggregateType });
        await (Task)method.Invoke(null, new object[] { repository, aggregate, token });
    }

    public async Task IndexProjectionAsync(AggregateBase aggregate, CancellationToken token = default)
    {
        var aggregateType = _aggregate.Aggregates.Get(aggregate.Metadata.Name);
        var repositoryType = typeof(ISearchProjectionRepository<>).MakeGenericType(aggregateType);
        var repository = _service.GetRequiredService(repositoryType);
        var method = repository.GetType().GetMethod(nameof(ISearchProjectionRepository<AggregateBase>.IndexAsync));
        await (Task)method.Invoke(repository, new object[] { aggregate, token });
    }

}