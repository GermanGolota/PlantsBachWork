using EventStore.Client;
using Microsoft.Extensions.Options;
using Plants.Infrastructure.Config;

namespace Plants.Infrastructure.Helpers;

public class EventStoreClientFactory
{
    private readonly IOptions<ConnectionConfig> _options;

    public EventStoreClientFactory(IOptions<ConnectionConfig> options)
    {
        _options = options;
    }

    public EventStoreClient Create()
    {
        var settings = EventStoreClientSettings.Create(_options.Value.EventStoreConnection);
        var client = new EventStoreClient(settings);
        return client;
    }
}
