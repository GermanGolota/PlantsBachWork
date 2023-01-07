using EventStore.Client;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plants.Domain.Infrastructure.Config;
using Plants.Services.Infrastructure.Encryption;

namespace Plants.Aggregates.Infrastructure.Domain;

internal class EventStoreClientSettingsFactory
{
    private readonly IOptions<ConnectionConfig> _options;
    private readonly ILoggerFactory _factory;
    private readonly IIdentityProvider _identity;
    private readonly IEnumerable<Interceptor> _interceptors;
    private readonly SymmetricEncrypter _encrypter;

    public EventStoreClientSettingsFactory(IOptions<ConnectionConfig> options, ILoggerFactory factory, IIdentityProvider identity, IEnumerable<Interceptor> interceptors, SymmetricEncrypter encrypter)
    {
        _options = options;
        _factory = factory;
        _identity = identity;
        _interceptors = interceptors;
        _encrypter = encrypter;
    }

    public EventStoreClientSettings CreateFor(EventStoreClientSettingsType type)
    {
        var options = _options.Value;
        var settings = EventStoreClientSettings.Create(options.EventStore.Template);
        switch (type)
        {
            case EventStoreClientSettingsType.User:
                var identity = _identity.Identity!;
                if (identity is not null)
                {
                    settings.DefaultCredentials = new UserCredentials(identity.UserName, _encrypter.Decrypt(identity.Hash));
                }
                break;
            case EventStoreClientSettingsType.Service:
                var creds = options.EventStore.Creds ?? options.DefaultCreds;
                settings.DefaultCredentials = new UserCredentials(creds.Username, creds.Password);
                break;
            default:
                throw new NotSupportedException($"Setting type - '{type}' is not supported");
        }

        settings.DefaultDeadline = TimeSpan.FromSeconds(options.EventStore.TimeoutInSeconds);
        settings.LoggerFactory ??= _factory;
        settings.Interceptors ??= _interceptors;
        return settings;
    }
}

internal enum EventStoreClientSettingsType
{
    User,
    Service
}


