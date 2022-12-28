namespace Plants.Domain.Projection;

public interface ISearchQueryService<TAggregate, TParams> where TAggregate : AggregateBase
{
    Task<IEnumerable<TAggregate>> SearchAsync(TParams parameters, OneOf<SearchPager, SearchAll> searchOption);
}

public record SearchAll();
public record SearchPager(int StartFrom, int Size);