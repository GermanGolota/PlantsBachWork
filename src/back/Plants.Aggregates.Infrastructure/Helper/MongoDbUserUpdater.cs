﻿using MongoDB.Bson;
using Plants.Aggregates.Services;
using Plants.Domain.Infrastructure.Services;

namespace Plants.Aggregates.Infrastructure.Helper;

internal class MongoDbUserUpdater : IUserUpdater
{
    private readonly IMongoClientFactory _factory;

    public MongoDbUserUpdater(IMongoClientFactory factory)
    {
        _factory = factory;
    }

    public async Task ChangeRole(string username, string fullName, UserRole[] oldRoles, UserRole changedRole)
    {
        var shouldRemoveRole = oldRoles.Contains(changedRole);
        var document =
        BsonDocument.Parse(
            $$"""
                {
                    "{{(shouldRemoveRole ? "revokeRolesFromUser" : "grantRolesToUser")}}": "{{username}}",
                    "roles": [{"role":"{{changedRole}}", "db": "admin"}]
                }
            """);
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

        await RunDocumentCommand(document); 
    }

    public async Task UpdatePassword(string username, string oldPassword, string newPassword)
    {
        var document = new BsonDocument
        {
            { "updateUser", username },
            { "pwd", newPassword }
        };

        await RunDocumentCommand(document);
    }

    private async Task RunDocumentCommand(BsonDocument document)
    {
        var db = _factory.GetDatabase("admin");
        var result = await db.RunCommandAsync<BsonDocument>(document);
        if (HasFailed(result))
        {
            throw new Exception(result.ToString());
        }
    }

    private bool HasFailed(BsonDocument resultDoc)
    {
        bool result;
        if (resultDoc.TryGetElement("ok", out var okElem))
        {
            var okValue = resultDoc.GetElement("ok").Value.AsDouble;
            result = okValue < 0.95 || okValue > 1.05;
        }
        else
        {
            result = true;
        }
        return result;
    }

}
