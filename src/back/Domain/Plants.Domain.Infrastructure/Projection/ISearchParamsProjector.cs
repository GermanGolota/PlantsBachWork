using Nest;

namespace Plants.Domain.Infrastructure;

public interface ISearchParamsProjector<TAggregate, TParams> where TAggregate : AggregateBase where TParams : ISearchParams
{
    SearchDescriptor<TAggregate> ProjectParams(TParams parameters, SearchDescriptor<TAggregate> desc);
}