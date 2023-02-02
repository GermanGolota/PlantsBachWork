namespace Plants.Domain.Infrastructure;

public interface IProjectionsUpdater
{
    Task<AggregateBase> UpdateProjectionAsync(AggregateDescription desc, DateTime? asOf = null, CancellationToken token = default);
}
