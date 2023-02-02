using EventStore.Client;

namespace Plants.Domain.Infrastructure;

public interface IEventStoreClientFactory
{
    EventStoreClient Create();
}