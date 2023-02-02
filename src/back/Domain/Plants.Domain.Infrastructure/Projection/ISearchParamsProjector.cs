using Nest;
using Plants.Domain.Aggregate;

namespace Plants.Domain.Infrastructure.Projection;

public interface ISearchParamsProjector<TAggregate, TParams> where TAggregate : AggregateBase where TParams : ISearchParams
{
    SearchDescriptor<TAggregate> ProjectParams(TParams parameters, SearchDescriptor<TAggregate> desc);
}