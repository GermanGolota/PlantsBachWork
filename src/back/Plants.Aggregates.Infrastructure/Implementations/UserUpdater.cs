using Plants.Aggregates.Abstractions;
using Plants.Aggregates.Infrastructure.Helper;
using Plants.Shared.Model;
using System.Data;

namespace Plants.Aggregates.Infrastructure.Implementations;

internal class UserUpdater : IUserUpdater
{
    private readonly EventStoreUserUpdater _eventStore;
    private readonly MongoDbUserUpdater _mongoDb;
    private readonly ElasticSearchUserUpdater _elasticSearch;

    private IEnumerable<IUserUpdater> Updaters()
    {
        yield return _eventStore;
        yield return _mongoDb;
        yield return _elasticSearch;
    }

    public UserUpdater(EventStoreUserUpdater eventStore, MongoDbUserUpdater mongoDb, ElasticSearchUserUpdater elasticSearch)
    {
        _eventStore = eventStore;
        _mongoDb = mongoDb;
        _elasticSearch = elasticSearch;
    }

    public async Task ChangeRoleAsync(string username, string fullName, UserRole[] oldRoles, UserRole changedRole, CancellationToken token = default)
    {
        var tasks = Updaters()
            .Select(updater => updater.ChangeRoleAsync(username, fullName, oldRoles, changedRole, token));
        await Task.WhenAll(tasks);
    }

    public async Task CreateAsync(string username, string password, string fullName, UserRole[] roles, CancellationToken token = default)
    {
        var tasks = Updaters()
            .Select(updater => updater.CreateAsync(username, password, fullName, roles, token));
        await Task.WhenAll(tasks);
    }

    public async Task UpdatePasswordAsync(string username, string oldPassword, string newPassword, CancellationToken token = default)
    {
        var tasks = Updaters()
            .Select(updater => updater.UpdatePasswordAsync(username, oldPassword, newPassword, token));
        await Task.WhenAll(tasks);
    }
}
