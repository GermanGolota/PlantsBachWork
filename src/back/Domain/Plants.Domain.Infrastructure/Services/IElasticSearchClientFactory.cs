using Nest;

namespace Plants.Domain.Infrastructure.Services;

public interface IElasticSearchClientFactory
{
    ElasticClient Create();
}
