using EventStore.Client;
using Plants.Aggregates.Infrastructure.Helper;
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
        new EventStoreUserManagementClient(_settingsFactory.CreateFor(EventStoreClientSettingsType.User));
}
