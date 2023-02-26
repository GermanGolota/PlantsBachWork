namespace Plants.Domain;

public interface ISearchQueryService<TAggregate, TParams> where TAggregate : AggregateBase where TParams : ISearchParams
{
    Task<IEnumerable<TAggregate>> SearchAsync(TParams parameters, QueryOptions searchOption, CancellationToken token = default);
}

public interface ISearchParams
{

}

public abstract record QueryOptions
{
    public sealed record All : QueryOptions;
    public sealed record Pager(int StartFrom, int Size) : QueryOptions;
}

