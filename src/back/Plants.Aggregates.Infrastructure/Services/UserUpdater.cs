using Plants.Aggregates.Infrastructure.Helper;
using Plants.Aggregates.Services;

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

    public async Task ChangeRoleAsync(string username, string fullName, UserRole[] oldRoles, UserRole changedRole, CancellationToken token = default)
    {
        await _eventStore.ChangeRoleAsync(username, fullName, oldRoles, changedRole, token);
        await _mongoDb.ChangeRoleAsync(username, fullName, oldRoles, changedRole, token);
        await _elasticSearch.ChangeRoleAsync(username, fullName, oldRoles, changedRole, token);
    }

    public async Task CreateAsync(string username, string password, string fullName, UserRole[] roles, CancellationToken token = default)
    {
        await _eventStore.CreateAsync(username, password, fullName, roles, token);
        await _mongoDb.CreateAsync(username, password, fullName, roles, token);
        await _elasticSearch.CreateAsync(username, password, fullName, roles, token);
    }

    public async Task UpdatePasswordAsync(string username, string oldPassword, string newPassword, CancellationToken token = default)
    {
        await _eventStore.UpdatePasswordAsync(username, oldPassword, newPassword, token);
        await _mongoDb.UpdatePasswordAsync(username, oldPassword, newPassword, token);
        await _elasticSearch.UpdatePasswordAsync(username, oldPassword, newPassword, token);
    }
}
