using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure.Helpers;

internal static class InfrastructureHelpers
{
    private readonly static Lazy<AggregateHelper> _aggregateHelper = new(() => new AggregateHelper(Shared.Helper.Helpers.Type));
    public static AggregateHelper Aggregate => _aggregateHelper.Value;
}
