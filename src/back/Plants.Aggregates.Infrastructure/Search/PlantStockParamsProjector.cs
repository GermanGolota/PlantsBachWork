using Nest;
using Plants.Aggregates.PlantStocks;
using Plants.Aggregates.Search;

namespace Plants.Aggregates.Infrastructure.Search;

internal class PlantStockParamsProjector : ISearchParamsProjector<PlantStock, PlantStockParams>
{
    private readonly IIdentityProvider _identityProvider;

    public PlantStockParamsProjector(IIdentityProvider identityProvider)
    {
        _identityProvider = identityProvider;
    }

    public SearchDescriptor<PlantStock> ProjectParams(PlantStockParams parameters, SearchDescriptor<PlantStock> desc)
    {
        var username = _identityProvider.Identity!.UserName;
        return desc.Query(q => parameters.IsMine 
            ? q.Term(t => t.Field(f => f.CaretakerUsername).Value(username))
            : q.MatchAll());
    }
} 
