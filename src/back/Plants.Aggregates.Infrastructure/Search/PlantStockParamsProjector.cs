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

    public SearchDescriptor<PlantStock> ProjectParams(PlantStockParams parameters, SearchDescriptor<PlantStock> desc) =>
        desc.Query(q =>
        q.Bool(b => b.Must(
                _ => parameters.IsMine
                ? _.Term(t => t.Field(f => f.Caretaker.Login).Value(_identityProvider.Identity!.UserName))
                : _.MatchAll(),
                _ => _.Term(f => f.Field(_ => _.BeenPosted).Value(false)))));
}
