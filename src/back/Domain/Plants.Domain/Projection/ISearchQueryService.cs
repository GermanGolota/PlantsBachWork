﻿namespace Plants.Domain.Projection;

public interface ISearchQueryService<TAggregate, TParams> where TAggregate : AggregateBase where TParams : ISearchParams
{
    Task<IEnumerable<TAggregate>> SearchAsync(TParams parameters, OneOf<SearchPager, SearchAll> searchOption);
}

public interface ISearchParams
{

}

public record SearchAll();
public record SearchPager(int StartFrom, int Size);