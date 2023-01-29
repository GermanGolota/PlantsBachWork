using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Plants.Domain.Infrastructure.Config;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Infrastructure.Services;
using Plants.Services.Infrastructure.Encryption;
using System.Reflection;

namespace Plants.Aggregates.Infrastructure.Domain;

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

    public ElasticClient Create()
    {
        var identity = _identity.Identity!;
        var uri = new Uri(string.Format(_connection.ElasticSearch.Template, identity.UserName, _encrypter.Decrypt(identity.Hash)));
        var pool = new SingleNodeConnectionPool(uri);
        var settings =
            new ConnectionSettings(pool,
                 sourceSerializer: (builtin, settings) => new CustomJsonSerializer(builtin, settings))
            .BasicAuthentication(identity.UserName, _encrypter.Decrypt(identity.Hash))
            ;


        settings.DisableDirectStreaming();
        return new ElasticClient(settings);
    }

    private class PrivateSetterContractResolver : ConnectionSettingsAwareContractResolver
    {
        public PrivateSetterContractResolver(IConnectionSettingsValues connectionSettings)
            : base(connectionSettings) { }

        protected override JsonProperty CreateProperty(
                 MemberInfo member,
                 MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }

    }

    private class CustomJsonSerializer : ConnectionSettingsAwareSerializerBase
    {
        public CustomJsonSerializer(IElasticsearchSerializer builtinSerializer, IConnectionSettingsValues connectionSettings)
            : base(builtinSerializer, connectionSettings) { }

        protected override JsonSerializerSettings CreateJsonSerializerSettings() =>
            JsonSerializerSettingsContext.Settings;

        protected override ConnectionSettingsAwareContractResolver CreateContractResolver() =>
            new PrivateSetterContractResolver(ConnectionSettings);
    }
}




