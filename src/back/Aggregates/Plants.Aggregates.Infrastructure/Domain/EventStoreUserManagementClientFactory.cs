using EventStore.Client;

namespace Plants.Aggregates.Infrastructure;

internal class EventStoreUserManagementClientFactory : IEventStoreUserManagementClientFactory
{
    private readonly EventStoreClientSettingsFactory _settingsFactory;

    public EventStoreUserManagementClientFactory(EventStoreClientSettingsFactory settingsFactory)
    {
        _settingsFactory = settingsFactory;
    }

    public EventStoreUserManagementClient Create() =>
        new(_settingsFactory.CreateFor(EventStoreClientSettingsType.User));
}
