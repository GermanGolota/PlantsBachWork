using EventStore.Client;

namespace Plants.Aggregates.Infrastructure;

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
