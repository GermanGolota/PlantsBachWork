using Plants.Domain.Aggregate;

namespace Plants.Domain.Infrastructure.Subscription;

public interface IProjectionsUpdater
{
    Task<AggregateBase> UpdateProjectionAsync(AggregateDescription desc, DateTime? asOf = null, CancellationToken token = default);
}
