using Plants.Aggregates.Infrastructure.Helper;
using Plants.Aggregates.Services;
using System.Data;

namespace Plants.Aggregates.Infrastructure.Services;

internal class UserUpdater : IUserUpdater
{
    private readonly EventStoreUserUpdater _eventStore;
    private readonly MongoDbUserUpdater _mongoDb;
    private readonly ElasticSearchUserUpdater _elasticSearch;

    public UserUpdater(EventStoreUserUpdater eventStore, MongoDbUserUpdater mongoDb, ElasticSearchUserUpdater elasticSearch)
    {
        _eventStore = eventStore;
        _mongoDb = mongoDb;
        _elasticSearch = elasticSearch;
    }

    public async Task ChangeRoleAsync(string username, string fullName, UserRole[] oldRoles, UserRole changedRole)
    {
        await _eventStore.ChangeRoleAsync(username, fullName, oldRoles, changedRole);
        await _mongoDb.ChangeRoleAsync(username, fullName, oldRoles, changedRole);
        await _elasticSearch.ChangeRoleAsync(username, fullName, oldRoles, changedRole);
    }

    public async Task CreateAsync(string username, string password, string fullName, UserRole[] roles)
    {
        await _eventStore.CreateAsync(username, password, fullName, roles);
        await _mongoDb.CreateAsync(username, password, fullName, roles);
        await _elasticSearch.CreateAsync(username, password, fullName, roles);
    }

    public async Task UpdatePasswordAsync(string username, string oldPassword, string newPassword)
    {
        await _eventStore.UpdatePasswordAsync(username, oldPassword, newPassword);
        await _mongoDb.UpdatePasswordAsync(username, oldPassword, newPassword);
        await _elasticSearch.UpdatePasswordAsync(username, oldPassword, newPassword);
    }
}
