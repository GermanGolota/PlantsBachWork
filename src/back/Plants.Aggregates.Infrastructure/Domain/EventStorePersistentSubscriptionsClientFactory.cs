using EventStore.Client;
using Plants.Domain.Infrastructure.Services;

namespace Plants.Aggregates.Infrastructure.Domain;

internal class EventStorePersistentSubscriptionsClientFactory : IEventStorePersistentSubscriptionsClientFactory
{
    private readonly EventStoreClientSettingsFactory _factory;

    public EventStorePersistentSubscriptionsClientFactory(EventStoreClientSettingsFactory factory)
    {
        _factory = factory;
    }

    public EventStorePersistentSubscriptionsClient Create() =>
        new(_factory.CreateFor(EventStoreClientSettingsType.Service));
}
