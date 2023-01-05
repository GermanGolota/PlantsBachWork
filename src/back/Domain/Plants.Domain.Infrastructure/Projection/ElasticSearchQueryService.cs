using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plants.Domain.Infrastructure.Extensions;
using Plants.Domain.Infrastructure.Services;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure.Projection;

public class ElasticSearchQueryService<TAggregate, TParams> : ISearchQueryService<TAggregate, TParams> where TAggregate : AggregateBase where TParams : ISearchParams
{
    private readonly IElasticSearchClientFactory _clientFactory;
    private readonly AggregateHelper _helper;
    private readonly IServiceProvider _provider;
    private readonly ILoggerFactory _loggerFactory;

    public ElasticSearchQueryService(IElasticSearchClientFactory clientFactory, AggregateHelper helper, IServiceProvider provider, ILoggerFactory loggerFactory)
    {
        _clientFactory = clientFactory;
        _helper = helper;
        _provider = provider;
        _loggerFactory = loggerFactory;
    }

    public async Task<IEnumerable<TAggregate>> SearchAsync(TParams parameters, OneOf<SearchPager, SearchAll> searchOption, CancellationToken token = default)
    {
        var aggregateName = _helper.Aggregates.Get(typeof(TAggregate));
        var client = _clientFactory.Create();
        var projector = _provider.GetService<ISearchParamsProjector<TAggregate, TParams>>();
        if (projector is null)
        {
            throw new Exception($"Cannot search '{aggregateName}' with '{typeof(TParams).Name}' - no projector");
        }

        var result = await client.SearchAsync<TAggregate>(s =>
        {
            s.Index(aggregateName.ToIndexName());
            searchOption.Match(page => { s.From(page.StartFrom).Size(page.Size); }, all => { });
            projector.ProjectParams(parameters, s);
            return s;
        }, ct: token);

        result.Process(_loggerFactory.CreateLogger(GetType()), aggregateName, "Search");
        return result.Documents;

    }
}
