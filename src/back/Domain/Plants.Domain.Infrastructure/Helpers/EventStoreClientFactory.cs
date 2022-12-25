using EventStore.Client;
using Plants.Domain.Infrastructure.Services;

namespace Plants.Domain.Infrastructure.Helpers;

public class EventStoreClientFactory
{
    private readonly IEventStoreClientSettingsFactory _settingsFactory;

    public EventStoreClientFactory(IEventStoreClientSettingsFactory settingsFactory)
    {
        _settingsFactory = settingsFactory;
    }

    public EventStoreClient Create() =>
        new(_settingsFactory.CreateFor(EventStoreClientSettingsType.User));
}
