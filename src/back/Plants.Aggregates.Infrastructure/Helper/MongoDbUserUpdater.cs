using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Plants.Aggregates.Infrastructure.Encryption;
using Plants.Aggregates.Services;
using Plants.Core;
using Plants.Infrastructure.Config;

namespace Plants.Aggregates.Infrastructure.Helper;

internal class MongoDbUserUpdater : IUserUpdater
{
    private readonly IOptions<ConnectionConfig> _options;
    private readonly IIdentityProvider _identity;
    private readonly SymmetricEncrypter _encrypter;

    public MongoDbUserUpdater(IOptions<ConnectionConfig> options, IIdentityProvider identity, SymmetricEncrypter encrypter)
    {
        _options = options;
        _identity = identity;
        _encrypter = encrypter;
    }

    public async Task ChangeRole(string username, string fullName, UserRole[] oldRoles, UserRole changedRole)
    {
    }

    public async Task Create(string username, string password, string fullName, UserRole[] roles)
    {
        var options = _options.Value;
        var client = new MongoClient(options.MongoDbConnection);
        var roleArray = new BsonArray(roles.Select(x => x.ToString()));
        var user = new BsonDocument {
            { "createUser", username },
            { "pwd", password },
            { "roles", roleArray }
        };

        var db = client
            .GetDatabase(options.MongoDbDatabaseName);

        await db.RunCommandAsync<BsonDocument>(user);

    }

    public async Task UpdatePassword(string username, string oldPassword, string newPassword)
    {
    }
}
