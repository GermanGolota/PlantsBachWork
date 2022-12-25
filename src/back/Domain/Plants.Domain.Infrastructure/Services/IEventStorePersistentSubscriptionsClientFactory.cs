using EventStore.Client;

namespace Plants.Domain.Infrastructure.Services;

public interface IEventStorePersistentSubscriptionsClientFactory
{
    EventStorePersistentSubscriptionsClient Create();
}
