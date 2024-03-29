﻿using Microsoft.Extensions.Logging;

namespace Plants.Domain.Infrastructure;

internal class ElasticSearchProjectionRepository<T> : ISearchProjectionRepository<T> where T : AggregateBase
{
    private readonly IElasticSearchClientFactory _factory;
    private readonly AggregateHelper _helper;
    private readonly ILoggerFactory _loggerFactory;

    public ElasticSearchProjectionRepository(IElasticSearchClientFactory factory, AggregateHelper helper, ILoggerFactory loggerFactory)
    {
        _factory = factory;
        _helper = helper;
        _loggerFactory = loggerFactory;
    }

    public async Task IndexAsync(T item, CancellationToken token = default)
    {
        var aggregateName = _helper.Aggregates.Get(typeof(T));
        var client = _factory.Create();
        var response = await client.IndexAsync(item, _ => _.Index(aggregateName.ToIndexName()), token);
        response.Process(_loggerFactory.CreateLogger(GetType()), aggregateName, "Index");
    }
}
