using Nest;

namespace Plants.Aggregates.Infrastructure;

internal class PlantPostParamsProjector : ISearchParamsProjector<PlantPost, PlantPostParams>
{
    public SearchDescriptor<PlantPost> ProjectParams(PlantPostParams parameters, SearchDescriptor<PlantPost> desc) =>
        desc.Query(q => q.Bool(b => b.Must(
            u => u.Term(m => m.Field(_ => _.IsRemoved).Value(false)),
            u => u.Term(m => m.Field(_ => _.IsOrdered).Value(false)),
            u => u.FilterOrAll(parameters.PlantName, (c, filter) =>
                c.Fuzzy(f => f.Field(_ => _.Stock.Information.PlantName).Value(filter))),
            /*u => u.FilterOrAll(parameters.Regions, (c, filter) =>
                c.Terms(f => f.Field(_ => _.Stock.Information.RegionNames).Terms(filter))),
            u => u.FilterOrAll(parameters.Groups, (c, filter) =>
                c.Terms(f => f.Field(_ => _.Stock.Information.GroupName).Terms(filter))),
            u => u.FilterOrAll(parameters.Soils, (c, filter) =>
                c.Terms(f => f.Field(_ => _.Stock.Information.SoilName).Terms(filter))),*/
            u => u.FilterOrAll(parameters.LastDate, (c, filter) =>
                c.Range(r => r.Field(_ => _.Stock.CreatedTime).LessThan(new DateTimeOffset(filter!.Value).ToUnixTimeMilliseconds()))),
            u =>
            {
                return (parameters.LowerPrice is null, parameters.TopPrice is null) switch
                {
                    (true, true) => u.MatchAll(),
                    (true, false) => u.Range(_ => _.Field(f => f.Price).LessThan((double?)parameters.TopPrice)),
                    (false, true) => u.Range(_ => _.Field(f => f.Price).GreaterThan((double?)parameters.LowerPrice)),
                    (false, false) => u.Range(_ => _.Field(f => f.Price).GreaterThan((double?)parameters.LowerPrice).LessThan((double?)parameters.TopPrice))
                };
            }
            )));

}

public static class QueryContainerDescriptorExtensions
{
    public static QueryContainer FilterOrAll<TFilter, TItem>(
        this QueryContainerDescriptor<TItem> u, TFilter? filter, Func<QueryContainerDescriptor<TItem>, TFilter, QueryContainer> filterFunc) where TItem : class
    {
        QueryContainer result;
        if (filter is null)
        {
            result = u.MatchAll();
        }
        else
        {
            result = filterFunc(u, filter);
        }
        return result;
    }
}
