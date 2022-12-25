using EventStore.Client;
using Plants.Domain.Infrastructure.Services;

namespace Plants.Aggregates.Infrastructure.Domain;

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
