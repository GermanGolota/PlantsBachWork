using EventStore.Client;

namespace Plants.Domain.Infrastructure;

public interface IEventStorePersistentSubscriptionsClientFactory
{
    EventStorePersistentSubscriptionsClient Create();
}
