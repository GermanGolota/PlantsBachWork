using Nest;

namespace Plants.Aggregates.Infrastructure;

internal class PlantOrderParamsProjector : ISearchParamsProjector<PlantOrder, PlantOrderParams>
{
    private readonly IIdentityProvider _identity;

    public PlantOrderParamsProjector(IIdentityProvider identity)
    {
        _identity = identity;
    }

    public SearchDescriptor<PlantOrder> ProjectParams(PlantOrderParams parameters, SearchDescriptor<PlantOrder> desc)
    {
        var username = _identity.Identity!.UserName;
        var validStatuses = Enum.GetValues<OrderStatus>().ToList();
        validStatuses.Remove(OrderStatus.Rejected);
        return desc.Query(q => q.Bool(b => b.Must(
                u => u.Terms(f => f.Field(_ => _.Status).Terms(validStatuses)),
                u => parameters.OnlyMine ? u.Term(t => t.Field(_ => _.Buyer.Login).Value(username)) : u.MatchAll()
                )));
    }
}
