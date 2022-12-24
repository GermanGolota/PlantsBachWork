using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Plants.Aggregates.Services;
using Plants.Domain.Infrastructure.Config;
using Plants.Domain.Infrastructure.Services;
using Plants.Services.Infrastructure.Encryption;
using Plants.Shared;

namespace Plants.Aggregates.Infrastructure.Helper;

internal class MongoDbUserUpdater : IUserUpdater
{
    private readonly IOptions<ConnectionConfig> _options;
    private readonly IMongoClientFactory _factory;
    private readonly IIdentityProvider _identity;
    private readonly SymmetricEncrypter _encrypter;

    public MongoDbUserUpdater(IOptions<ConnectionConfig> options, IIdentityProvider identity, SymmetricEncrypter encrypter, IMongoClientFactory factory)
    {
        _options = options;
        _identity = identity;
        _encrypter = encrypter;
        _factory = factory;
    }

    public async Task ChangeRole(string username, string fullName, UserRole[] oldRoles, UserRole changedRole)
    {
        var document = (oldRoles.Contains(changedRole)) switch
        {
            true => new BsonDocument
            {
                { "revokeRolesFromUser", username },
                { "roles", new BsonDocument
                    {
                        {"role", changedRole.ToString()},
                        {"db", _options.Value.MongoDbDatabaseName }
                    }
                }
            },
            false => new BsonDocument
            {
                { "grantRolesToUser", username },
                { "roles", new BsonArray(new []{ changedRole})}
            }
        };
        await RunDocumentCommand(document);
    }

    public async Task Create(string username, string password, string fullName, UserRole[] roles)
    {
        var roleArray = new BsonArray(roles.Select(x => x.ToString()));
        var document = new BsonDocument {
            { "createUser", username },
            { "pwd", password },
            { "roles", roleArray }
        };

        var result = await RunDocumentCommand(document);
    }

    public async Task UpdatePassword(string username, string oldPassword, string newPassword)
    {
        var document = new BsonDocument
        {
            { "updateUser", username },
            { "pwd", newPassword }
        };

        var result = await RunDocumentCommand(document);
    }

    private async Task<BsonDocument> RunDocumentCommand(BsonDocument document)
    {
        var db = _factory.GetDatabase("admin");
        return await db.RunCommandAsync<BsonDocument>(document);
    }

}
