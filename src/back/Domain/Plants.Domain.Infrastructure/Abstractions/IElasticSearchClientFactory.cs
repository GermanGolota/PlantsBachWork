using Nest;

namespace Plants.Domain.Infrastructure;

public interface IElasticSearchClientFactory
{
    ElasticClient Create();
}
