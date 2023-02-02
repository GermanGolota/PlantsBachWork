namespace Plants.Domain;

public interface ISearchQueryService<TAggregate, TParams> where TAggregate : AggregateBase where TParams : ISearchParams
{
    Task<IEnumerable<TAggregate>> SearchAsync(TParams parameters, OneOf<SearchPager, SearchAll> searchOption, CancellationToken token = default);
}

public interface ISearchParams
{

}

public record SearchAll();
public record SearchPager(int StartFrom, int Size);
