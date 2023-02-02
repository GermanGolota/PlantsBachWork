using EventStore.Client;

namespace Plants.Domain.Infrastructure;

public interface IEventStoreUserManagementClientFactory
{
    EventStoreUserManagementClient Create();
}