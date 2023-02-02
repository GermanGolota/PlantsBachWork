using Nest;

namespace Plants.Domain.Infrastructure;

public interface ISearchParamsOrderer<TAggregate, TParams> where TAggregate : AggregateBase where TParams : ISearchParams
{
    IPromise<IList<ISort>> OrderParams(TParams parameters, SortDescriptor<TAggregate> desc);
}