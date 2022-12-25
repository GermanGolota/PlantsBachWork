using EventStore.Client;

namespace Plants.Domain.Infrastructure.Services;

public interface IEventStoreClientFactory
{
    EventStoreClient Create();
}