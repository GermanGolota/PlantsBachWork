using Plants.Infrastructure.Domain.Helpers;
using Plants.Shared;

namespace Plants.Domain.Infrastructure;

internal static class InfrastructureHelpers
{
    private readonly static Lazy<AggregateHelper> _aggregateHelper = new(() => new AggregateHelper(Helpers.Type));
    public static AggregateHelper Aggregate => _aggregateHelper.Value;
}
