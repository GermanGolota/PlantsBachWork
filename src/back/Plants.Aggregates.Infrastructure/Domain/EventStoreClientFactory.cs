using EventStore.Client;
using Plants.Aggregates.Infrastructure.Helper;
using Plants.Domain.Infrastructure.Services;

namespace Plants.Aggregates.Infrastructure.Domain;

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

