using Plants.Domain.Infrastructure.Helpers;

namespace Plants.Domain.Infrastructure.Subscription;

internal class ProjectionsUpdater : IProjectionsUpdater
{
    private readonly RepositoriesCaller _caller;

    public ProjectionsUpdater(RepositoriesCaller caller)
    {
        _caller = caller;
    }

    public async Task<AggregateBase> UpdateProjectionAsync(AggregateDescription desc, DateTime? asOf = null, CancellationToken token = default)
    {
        var aggregate = await _caller.LoadAsync(desc, asOf, token);
        await _caller.InsertOrUpdateProjectionAsync(aggregate, token);
        await _caller.IndexProjectionAsync(aggregate, token);
        return aggregate;
    }
}
