using Nest;

namespace Plants.Aggregates.Infrastructure;

internal class PlantOrderParamsOrderer : ISearchParamsOrderer<PlantOrder, PlantOrderParams>
{
    public IPromise<IList<ISort>> OrderParams(PlantOrderParams parameters, SortDescriptor<PlantOrder> desc) =>
        desc.Field(_ => _.Field(agg => agg.Status).Ascending());
}
