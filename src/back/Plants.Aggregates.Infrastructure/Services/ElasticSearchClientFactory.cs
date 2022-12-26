using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using Plants.Domain.Infrastructure.Config;
using Plants.Domain.Infrastructure.Services;
using Plants.Services.Infrastructure.Encryption;

namespace Plants.Aggregates.Infrastructure.Services;

internal class ElasticSearchClientFactory : IElasticSearchClientFactory
{
    private readonly ConnectionConfig _connection;
    private readonly IIdentityProvider _identity;
    private readonly SymmetricEncrypter _encrypter;

    public ElasticSearchClientFactory(IOptions<ConnectionConfig> connection, IIdentityProvider identity, SymmetricEncrypter encrypter)
    {
        _connection = connection.Value;
        _identity = identity;
        _encrypter = encrypter;
    }

    public ElasticsearchClient Create()
    {
        var identity = _identity.Identity!;
        var uri = new Uri(String.Format(_connection.ElasticSearchConnectionTemplate, identity.UserName, _encrypter.Decrypt(identity.Hash)));
        var settings = new ElasticsearchClientSettings(uri);
        settings.DisableDirectStreaming();
        return new ElasticsearchClient(settings);
    }
}
