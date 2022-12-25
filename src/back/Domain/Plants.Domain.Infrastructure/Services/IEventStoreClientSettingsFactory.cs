using EventStore.Client;

namespace Plants.Domain.Infrastructure.Services;

public interface IEventStoreClientSettingsFactory
{
    EventStoreClientSettings CreateFor(EventStoreClientSettingsType type);
}

public enum EventStoreClientSettingsType
{
    User,
    Service
}
