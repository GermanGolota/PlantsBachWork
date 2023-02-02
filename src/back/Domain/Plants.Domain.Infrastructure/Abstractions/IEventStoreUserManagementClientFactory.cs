using EventStore.Client;

namespace Plants.Domain.Infrastructure.Services;

public interface IEventStoreUserManagementClientFactory
{
    EventStoreUserManagementClient Create();
}