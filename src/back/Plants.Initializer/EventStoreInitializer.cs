using EventStore.ClientAPI;

namespace Plants.Initializer;

internal class EventStoreInitializer
{
    private readonly IEventStoreConnection _connection;

    public EventStoreInitializer(IEventStoreConnection connection)
    {
        _connection = connection;
    }

    public async Task Initialize(AccessorsDefinition definiton)
    {

    }
}
