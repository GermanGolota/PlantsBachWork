using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;

namespace Plants.Domain.Infrastructure;

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

    public async Task<IEnumerable<TAggregate>> SearchAsync(TParams parameters, QueryOptions options, CancellationToken token = default)
    {
        var aggregateName = _helper.Aggregates.Get(typeof(TAggregate));
        var client = _clientFactory.Create();
        var projector = _provider.GetService<ISearchParamsProjector<TAggregate, TParams>>();
        if (projector is null)
        {
            throw new Exception($"Cannot search '{aggregateName}' with '{typeof(TParams).Name}' - no projector");
        }

        var orderer = _provider.GetService<ISearchParamsOrderer<TAggregate, TParams>>();

        var result = await client.SearchAsync<TAggregate>(s =>
        {
            s.Index(aggregateName.ToIndexName());

            if(options is QueryOptions.Pager pager)
            {
                s.From(pager.StartFrom).Size(pager.Size);
            }

            projector.ProjectParams(parameters, s);

            s.Sort(a =>
            {
                orderer?.OrderParams(parameters, a);
                return a.Field(agg => agg.Field(_ => _.Metadata.LastUpdateTime).Descending());
            });

            return s;
        }, ct: token);

        result.Process(_loggerFactory.CreateLogger(GetType()), aggregateName, "Search");
        return result.Documents;

    }
}
