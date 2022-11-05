using Plants.Infrastructure.Domain.Helpers;
using Plants.Shared;

namespace Plants.Domain.Infrastructure;

public static class InfraHelpers
{
    private readonly static Lazy<AggregateHelper> _aggregateHelper = new(() => new AggregateHelper(Helpers.Type));
    public static AggregateHelper Aggregate => _aggregateHelper.Value;
}
