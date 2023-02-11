using EventStore.Client;

namespace Plants.Aggregates.Infrastructure;

internal class EventStoreClientFactory : IEventStoreClientFactory
{
    private readonly EventStoreClientSettingsFactory _settingsFactory;

    public EventStoreClientFactory(EventStoreClientSettingsFactory settingsFactory)
    {
        _settingsFactory = settingsFactory;
    }

    public EventStoreClient Create() =>
        new(_settingsFactory.CreateFor(EventStoreClientSettingsType.User));
}

