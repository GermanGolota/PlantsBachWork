using Nest;

namespace Plants.Domain.Infrastructure.Projection;

public interface ISearchParamsProjector<TAggregate, TParams> where TAggregate : AggregateBase
{
    SearchDescriptor<TAggregate> ProjectParams(TParams parameters, SearchDescriptor<TAggregate> desc);
}
