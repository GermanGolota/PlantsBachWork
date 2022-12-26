using Elastic.Clients.Elasticsearch;

namespace Plants.Domain.Infrastructure.Services;

public interface IElasticSearchClientFactory
{
    ElasticsearchClient Create();
}
