using Microsoft.Extensions.DependencyInjection;
using Plants.Domain.Persistence;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure.Helpers;

internal class AggregateLoader
{
    private readonly AggregateHelper _aggregate;
    private readonly IServiceProvider _service;

    public AggregateLoader(AggregateHelper aggregate, IServiceProvider service)
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
}
