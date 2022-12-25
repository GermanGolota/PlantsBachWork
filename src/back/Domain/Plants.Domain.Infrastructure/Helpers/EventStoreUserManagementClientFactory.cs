using EventStore.Client;
using Plants.Domain.Infrastructure.Services;

namespace Plants.Domain.Infrastructure.Helpers;

public class EventStoreUserManagementClientFactory
{
    private readonly IEventStoreClientSettingsFactory _settingsFactory;

    public EventStoreUserManagementClientFactory(IEventStoreClientSettingsFactory settingsFactory)
	{
        _settingsFactory = settingsFactory;
    }

    public EventStoreUserManagementClient Create() =>
        new EventStoreUserManagementClient(_settingsFactory.CreateFor(EventStoreClientSettingsType.User));
}
