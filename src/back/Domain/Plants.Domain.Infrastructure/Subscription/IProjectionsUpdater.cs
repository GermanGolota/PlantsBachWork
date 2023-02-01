namespace Plants.Domain.Infrastructure.Subscription;

public interface IProjectionsUpdater
{
    Task UpdateProjectionAsync(AggregateDescription desc, DateTime? asOf = null, CancellationToken token = default);
}
