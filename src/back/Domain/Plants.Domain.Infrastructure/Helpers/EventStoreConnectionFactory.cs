using EventStore.ClientAPI;
using Microsoft.Extensions.Options;
using Plants.Infrastructure.Config;

namespace Plants.Infrastructure.Helpers;

internal class EventStoreConnectionFactory
{
    private readonly IOptions<ConnectionConfig> _options;

    public EventStoreConnectionFactory(IOptions<ConnectionConfig> options)
    {
        _options = options;
    }

    public IEventStoreConnection Create() =>
        EventStoreConnection.Create(new Uri(_options.Value.EventStoreConnection));
}
